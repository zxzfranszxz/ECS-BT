using SD.ECSBT.BehaviourTree.Data;
using SD.ECSBT.BehaviourTree.ECS.Blackboard;
using SD.ECSBT.BehaviourTree.ECS.Blackboard.Data;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Decorator;
using SD.ECSBT.BehaviourTree.ECS.Services;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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
            state.RequireForUpdate<BTDataElements>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var btDelegateData = SystemAPI.GetSingleton<BTDelegateData>();
            var btDataElements = SystemAPI.GetSingleton<BTDataElements>();
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
                    var oldInstanceAspect = SystemAPI.GetAspect<BTInstanceAspect>(oldBTInstance);
                    var oldBTData = oldInstanceAspect.GetBTData(in btDataElements);
                    BTHelper.CleanupBTInstance(ref state, ref entityManager, ref ecb, in oldInstanceAspect,
                        in oldBTData, in btDelegateData);
                }

                var btName = command.ValueRO.BTName;
                if (btName.IsEmpty)
                    btName = "BTDefault";

                if (btDataElements.BTDataMap.TryGetValue(btName, out var btData) == false)
                {
                    Debug.LogError($"BT: {btName} not found");
                    continue;
                }

                var btInstance = ecb.CreateEntity();
                ecb.SetName(btInstance, "BTInstance");
                ecb.AddComponent(btInstance, new BTInstanceData
                {
                    BehaviorTree = btName,
                    ActiveNodeId = 0,
                    ActiveNodeState = ActiveNodeState.None
                });
                ecb.AddComponent<BTLogicEnabled>(btInstance);
                ecb.AddComponent(btInstance, btOwner);

                aiControllerData.BTInstance = btInstance;
                ecb.AddComponent(owner, aiControllerData);

                // add Blackboard buffers
                ecb.AddBuffer<BBVarElement>(btInstance);

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
                        BehaviorTree = btName
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
            var btDataElements = SystemAPI.GetSingleton<BTDataElements>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;

            foreach (var btInstance in SystemAPI
                         .Query<BTInstanceAspect>())
            {
                var btData = btInstance.GetBTData(btDataElements);
                BTHelper.CleanupBTInstance(ref state, ref entityManager, ref ecb, in btInstance, btData, btDelegateData);
            }

            ecb.Dispose();
        }
    }
}