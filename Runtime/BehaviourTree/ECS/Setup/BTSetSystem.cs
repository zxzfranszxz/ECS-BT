using SD.ECSBT.BehaviourTree.Data;
using SD.ECSBT.BehaviourTree.ECS.Blackboard;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Decorator;
using SD.ECSBT.BehaviourTree.ECS.Services;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SD.ECSBT.BehaviourTree.ECS.Setup
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct BTSetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BTDelegateData>();
            state.RequireForUpdate<BTElement>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var btDelegateData = SystemAPI.GetSingleton<BTDelegateData>();
            var btElements = SystemAPI.GetSingletonBuffer<BTElement>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;

            foreach (var (command, entity) in SystemAPI.Query<RefRO<BTSetCommand>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);

                var owner = command.ValueRO.Target;
                if (!SystemAPI.Exists(owner)) continue;
                var aiControllerData = SystemAPI.GetComponent<AIControllerData>(command.ValueRO.Target);
                var btOwner = new BTOwner { Value = owner };

                // clean up old one
                var oldBTInstance = aiControllerData.BTInstance;
                if (SystemAPI.Exists(oldBTInstance))
                {
                    ref var btInstanceData = ref SystemAPI.GetComponentRW<BTInstanceData>(oldBTInstance).ValueRW;
                    ref var blackboard = ref SystemAPI.GetComponentRW<BlackboardData>(oldBTInstance).ValueRW;
                    ref readonly var oldBTData = ref SystemAPI.GetComponentRO<BTData>(btInstanceData.BehaviorTree).ValueRO;
                    BTHelper.CleanupBTInstance(ref state, ref entityManager, ref ecb, ref btInstanceData,
                        ref blackboard, oldBTData, owner, oldBTInstance, btDelegateData);
                }

                var btName = command.ValueRO.BTName;
                if (btName.IsEmpty)
                    btName = "BTDefault";

                BTHelper.GetBTree(btElements, btName, out var btEntity);
                if (btEntity == Entity.Null)
                {
                    Debug.LogError("Can't find bt entity");
                    continue;
                }

                var btInstance = ecb.CreateEntity();
                ecb.SetName(btInstance, "BTInstance");
                ecb.AddComponent(btInstance, new BTInstanceData
                {
                    BehaviorTree = btEntity,
                    ActiveNodeId = 0,
                    ActiveNodeState = ActiveNodeState.None
                });
                ecb.AddComponent<BTLogicEnabled>(btInstance);
                ecb.AddComponent(btInstance, btOwner);

                aiControllerData.BTInstance = btInstance;
                ecb.AddComponent(owner, aiControllerData);

                ref readonly var btData = ref SystemAPI.GetComponentRO<BTData>(btEntity).ValueRO;

                // create Blackboard
                var blackboardData = new BlackboardData
                {
                    BoolVars = new NativeHashMap<FixedString32Bytes, bool>(0, Allocator.Persistent),
                    IntVars = new NativeHashMap<FixedString32Bytes, int>(0, Allocator.Persistent),
                    FloatVars = new NativeHashMap<FixedString32Bytes, float>(0, Allocator.Persistent),
                    Float2Vars = new NativeHashMap<FixedString32Bytes, float2>(0, Allocator.Persistent),
                    Float3Vars = new NativeHashMap<FixedString32Bytes, float3>(0, Allocator.Persistent),
                    EntityVars = new NativeHashMap<FixedString32Bytes, Entity>(0, Allocator.Persistent),
                    QuaternionVars = new NativeHashMap<FixedString32Bytes, quaternion>(0, Allocator.Persistent),
                    StringVars = new NativeHashMap<FixedString32Bytes, FixedString32Bytes>(0, Allocator.Persistent)
                };
                ecb.AddComponent(btInstance, blackboardData);

                // add buffer for active AutoReturn nodes
                ecb.AddBuffer<BTActiveAutoReturnNodeElement>(btInstance);

                // create services
                ecb.AddBuffer<BTServiceElement>(btInstance);
                foreach (var node in btData.Nodes)
                {
                    if (node.NodeType != NodeType.Service) continue;
                    var service = ecb.CreateEntity();
                    ecb.SetName(service, "BTService");
                    ecb.AddComponent(service, node.NodeComponentType);
                    ecb.AddComponent(service, new NodeInstanceData
                    {
                        BTInstance = btInstance,
                        BTOwner = owner,
                        NodeId = node.Id
                    });
                    ecb.AddComponent<BTServiceEnabled>(service);
                    ecb.SetComponentEnabled<BTServiceEnabled>(service, false);
                    ecb.AddComponent<BTServiceTag>(service);
                    ecb.AddComponent(service, new BTServiceData
                    {
                        Frequency = node.FloatVars["Frequency"],
                        BTEntity = btEntity
                    });
                    ecb.AppendToBuffer(btInstance, new BTServiceElement
                    {
                        Service = service,
                        NodeId = node.Id
                    });
                }

                // subscribe decorators
                ecb.AddBuffer<AbortSubscriberElement>(btInstance);
                foreach (var node in btData.Nodes)
                {
                    if (node.NodeType != NodeType.Decorator) continue;
                    if (!node.IntVars.IsCreated || !node.IntVars.ContainsKey("AbortType")) continue;
                    var abortType = (AbortType)node.IntVars["AbortType"];
                    node.IntVars.TryGetValue("NotifyType", out var notifyTypeInt);
                    var notifyType = (NotifyType)notifyTypeInt;
                    ecb.AppendToBuffer(btInstance, new AbortSubscriberElement
                    {
                        NodeId = node.Id,
                        AbortType = abortType,
                        NotifyType = notifyType,
                        BlackboardId = node.StringVars["Blackboard"]
                    });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            var btDelegateData = SystemAPI.GetSingleton<BTDelegateData>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;
            
            foreach (var (btInstanceDataRW, blackboardDataRW, ownerRO, entity) in SystemAPI
                         .Query<RefRW<BTInstanceData>, RefRW<BlackboardData>, RefRO<BTOwner>>()
                         .WithEntityAccess())
            {
                ref readonly var btData = ref SystemAPI.GetComponentRO<BTData>(btInstanceDataRW.ValueRO.BehaviorTree).ValueRO;
                BTHelper.CleanupBTInstance(ref state, ref entityManager, ref ecb, ref btInstanceDataRW.ValueRW,
                    ref blackboardDataRW.ValueRW, btData, ownerRO.ValueRO.Value, entity, btDelegateData);
            }

            ecb.Dispose();
        }
    }
}