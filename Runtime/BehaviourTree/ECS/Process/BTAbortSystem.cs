using SD.ECSBT.BehaviourTree.Data;
using SD.ECSBT.BehaviourTree.ECS.Blackboard;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Decorator;
using SD.ECSBT.BehaviourTree.ECS.Services;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Process
{
    [UpdateInGroup(typeof(BTLogicSystemGroup))]
    public partial struct BTAbortSystem : ISystem
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
            var entityManager = state.EntityManager;

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (commandRO, entity) in SystemAPI.Query<RefRO<NotifyBlackboardVarCommand>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);

                ref readonly var command = ref commandRO.ValueRO;
                var btInstanceEntity = command.BTInstance;
                if (!SystemAPI.Exists(btInstanceEntity)) continue;
                var btInstance = SystemAPI.GetAspect<BTInstanceAspect>(btInstanceEntity);
                var subscribers = SystemAPI.GetBuffer<AbortSubscriberElement>(btInstanceEntity);
                var btData = btInstance.GetBTData(btDataElements);

                var interrupterNode = -1;
                foreach (var subscriber in subscribers)
                {
                    if (command.BlackboardId != subscriber.BlackboardId) continue;
                    if (subscriber.NodeId >= btInstance.InstanceDataRW.ValueRO.ActiveNodeId) continue;
                    if (subscriber.NotifyType != command.NotifyType) continue;
                    var relationAbortType =
                        GetNodeType(subscriber.NodeId, btInstance.InstanceDataRW.ValueRO.ActiveNodeId, in btData.Nodes);
                    if ((subscriber.AbortType & relationAbortType) == AbortType.None) continue;
                    if (interrupterNode > subscriber.NodeId) continue;
                    interrupterNode = subscriber.NodeId;
                }

                if (interrupterNode < 0) continue;
                btInstance.InstanceDataRW.ValueRW.ActiveNodeId = interrupterNode;
                btInstance.InstanceDataRW.ValueRW.PreviousNodeId = -1;
                btInstance.InstanceDataRW.ValueRW.RunState = BTRunState.Digging;
                btInstance.InstanceDataRW.ValueRW.ActiveNodeState = ActiveNodeState.None;

                // release active AutoReturn nodes below new node
                var autoReturnNodes = entityManager.GetBuffer<BTActiveAutoReturnNodeElement>(btInstanceEntity);

                for (var i = 0; i < autoReturnNodes.Length; i++)
                {
                    var element = autoReturnNodes[i];
                    if (btInstance.InstanceDataRW.ValueRO.ActiveNodeId > element.NodeId) continue;
                    btDelegateData.BTNodeReturnHandlerFunc.Invoke(ref state, ref ecb, in btInstance, btData, element.NodeId);
                    autoReturnNodes.RemoveAt(i);
                    i--;
                }

                // stop services below new node
                BTServiceHelper.DeactivateServicesBelowNode(ref entityManager, in interrupterNode, in btInstanceEntity);

                if (SystemAPI.HasComponent<BTActiveNodeLink>(btInstanceEntity))
                {
                    ecb.SetComponentEnabled<BTLogicEnabled>(btInstanceEntity, true);
                    ecb.DestroyEntity(SystemAPI.GetComponentRO<BTActiveNodeLink>(btInstanceEntity).ValueRO.ActiveNode);
                    ecb.RemoveComponent<BTActiveNodeLink>(btInstanceEntity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        private AbortType GetNodeType(in int nodeId, in int currentNodeId, in NativeArray<NodeData> btDataNodes)
        {
            var parentId = btDataNodes[currentNodeId].ParentId;
            var abortType = AbortType.LowerPriority;
            while (parentId > 0)
            {
                if (parentId == nodeId)
                {
                    abortType = AbortType.Self;
                    break;
                }

                parentId = btDataNodes[parentId].ParentId;
            }

            return abortType;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}