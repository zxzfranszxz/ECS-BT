using SD.ECSBT.BehaviourTree.Data;
using SD.ECSBT.BehaviourTree.ECS.Blackboard;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
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
            state.RequireForUpdate<BTElement>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var btElements = SystemAPI.GetSingletonBuffer<BTElement>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;

            foreach (var (command, entity) in SystemAPI.Query<RefRO<BTSetCommand>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                var aiController = SystemAPI.GetComponent<AIControllerLink>(command.ValueRO.Target).Controller;
                var aiControllerData = SystemAPI.GetComponentRO<AIControllerData>(aiController).ValueRO;
                var owner = SystemAPI.GetComponent<BTOwner>(aiController);
                
                
                // clean up old one
                var oldBTInstance = aiControllerData.BTInstance;
                if(SystemAPI.Exists(oldBTInstance))
                    BTHelper.CleanupBTInstance(ref entityManager, ref ecb, oldBTInstance);

                var btName = command.ValueRO.BTName;
                if (btName.IsEmpty)
                    btName = "DefaultBT";

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
                ecb.AddComponent(btInstance, owner);
                ecb.AddComponent(btInstance, new AIControllerLink { Controller = aiController });
                
                aiControllerData.BTInstance = btInstance;
                ecb.SetComponent(aiController, aiControllerData);

                ref readonly var btData = ref SystemAPI.GetComponentRO<BTData>(btEntity).ValueRO;

                // create Blackboard
                var blackboardData = new Blackboard.BlackboardData
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

                // create services
                ecb.AddBuffer<BTServiceElement>(btInstance);
                foreach (var node in btData.Nodes)
                {
                    if (node.NodeType != NodeType.Service) continue;
                    var service = ecb.CreateEntity();
                    ecb.SetName(service, "BTService");
                    ecb.AddComponent(service, node.NodeComponentType);
                    ecb.AddComponent<BTServiceEnabled>(service);
                    ecb.SetComponentEnabled<BTServiceEnabled>(service, false);
                    ecb.AddComponent<BTServiceTag>(service);
                    ecb.AddComponent(service, new BTServiceData
                    {
                        Frequency = btData.NodeVars[node.Id].FloatVars["Frequency"],
                        BTEntity = btEntity
                    });
                    ecb.AddComponent(service, new BTInstanceLink { BTInstance = btInstance });
                    ecb.AddComponent(service, owner);
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
                    if (!btData.NodeVars[node.Id].IntVars.ContainsKey("AbortType")) continue;
                    var abortType = (AbortType)btData.NodeVars[node.Id].IntVars["AbortType"];
                    btData.NodeVars[node.Id].IntVars.TryGetValue("NotifyType", out var notifyTypeInt);
                    var notifyType = (NotifyType)notifyTypeInt;
                    ecb.AppendToBuffer(btInstance, new AbortSubscriberElement
                    {
                        NodeId = node.Id,
                        AbortType = abortType,
                        NotifyType = notifyType,
                        BlackboardId = btData.NodeVars[node.Id].StringVars["Blackboard"]
                    });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;

            foreach (var (_, entity) in SystemAPI.Query<RefRO<BTInstanceData>>().WithEntityAccess())
            {
                BTHelper.CleanupBTInstance(ref entityManager, ref ecb, entity);
            }

            ecb.Dispose();
        }
    }
}