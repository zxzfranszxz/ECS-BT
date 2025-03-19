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
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var btDelegateData = SystemAPI.GetSingleton<BTDelegateData>();
            var entityManager = state.EntityManager;

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (commandRO, entity) in SystemAPI.Query<RefRO<NotifyBlackboardVarCommand>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);

                ref readonly var command = ref commandRO.ValueRO;
                var btInstance = command.BTInstance;
                if (!SystemAPI.Exists(btInstance)) continue;
                var subscribers = SystemAPI.GetBuffer<AbortSubscriberElement>(btInstance);
                ref var btInstanceData = ref SystemAPI.GetComponentRW<BTInstanceData>(btInstance).ValueRW;
                ref var blackboard = ref SystemAPI.GetComponentRW<BlackboardData>(btInstance).ValueRW;
                var owner = SystemAPI.GetComponent<BTOwner>(btInstance).Value;
                ref readonly var btData = ref SystemAPI.GetComponentRO<BTData>(btInstanceData.BehaviorTree).ValueRO;

                var interrupterNode = -1;
                foreach (var subscriber in subscribers)
                {
                    if (command.BlackboardId != subscriber.BlackboardId) continue;
                    if (subscriber.NodeId >= btInstanceData.ActiveNodeId) continue;
                    if (subscriber.NotifyType != command.NotifyType) continue;
                    var relationAbortType =
                        GetNodeType(subscriber.NodeId, btInstanceData.ActiveNodeId, in btData.Nodes);
                    if ((subscriber.AbortType & relationAbortType) == AbortType.None) continue;
                    if (interrupterNode > subscriber.NodeId) continue;
                    interrupterNode = subscriber.NodeId;
                }

                if (interrupterNode < 0) continue;
                btInstanceData.ActiveNodeId = interrupterNode;
                btInstanceData.PreviousNodeId = -1;
                btInstanceData.RunState = BTRunState.Digging;
                btInstanceData.ActiveNodeState = ActiveNodeState.None;

                // release active AutoReturn nodes below new node
                var autoReturnNodes = entityManager.GetBuffer<BTActiveAutoReturnNodeElement>(btInstance);

                for (var i = 0; i < autoReturnNodes.Length; i++)
                {
                    var element = autoReturnNodes[i];
                    if (btInstanceData.ActiveNodeId > element.NodeId) continue;
                    btDelegateData.BTNodeReturnHandlerFunc.Invoke(ref state, ref ecb, ref btInstanceData,
                        ref blackboard, btData, owner, btInstance, element.NodeId);
                    autoReturnNodes.RemoveAt(i);
                    i--;
                }

                // stop services below new node
                BTServiceHelper.DeactivateServicesBelowNode(ref entityManager, in interrupterNode, in btInstance);

                if (SystemAPI.HasComponent<BTActiveNodeLink>(btInstance))
                {
                    ecb.SetComponentEnabled<BTLogicEnabled>(btInstance, true);
                    ecb.DestroyEntity(SystemAPI.GetComponentRO<BTActiveNodeLink>(btInstance).ValueRO.ActiveNode);
                    ecb.RemoveComponent<BTActiveNodeLink>(btInstance);
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