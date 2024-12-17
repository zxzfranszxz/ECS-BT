using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.ECS.Services;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Process
{
    [UpdateInGroup(typeof(BTLogicSystemGroup))]
    public partial struct BTProcessSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;
            
            foreach (var (btInstanceRW, blackboardDataRW, ownerRO, entity) in SystemAPI.Query<RefRW<BTInstanceData>, RefRW<Blackboard.BlackboardData>, RefRO<BTOwner>>()
                         .WithAll<BTLogicEnabled>().WithEntityAccess())
            {
                ref var btInstanceData = ref btInstanceRW.ValueRW;
                ref var blackboardData = ref blackboardDataRW.ValueRW;
                ref readonly var btData = ref SystemAPI.GetComponentRO<BTData>(btInstanceData.BehaviorTree).ValueRO;
                var owner = ownerRO.ValueRO.Value;
                
                if (btInstanceData is { RunState: BTRunState.None, ActiveNodeId: 0 })
                {
                    btInstanceData.RunState = BTRunState.Digging;
                }
                
                while (true)
                {
                    if (btInstanceData.RunState == BTRunState.Digging)
                    {
                        //todo
                        // BTNodeRunHandler.RunNode(ref state, ref this, ref btInstanceData, ref blackboardData, in btData, in owner, in entity, ref entityManager, ref ecb, out var result);
                        // btInstanceData.ActiveNodeState = result;
                        // if (result == ActiveNodeState.Running)
                        // {
                        //     SystemAPI.SetComponentEnabled<BTLogicEnabled>(entity, false);
                        //     break;
                        // }
                        // BTNodeResultHandler.ReturnNodeResult(ref btInstanceData, in btData);
                    }
                    else if(btInstanceData.RunState == BTRunState.Returning)
                    {
                        if (btData.Nodes[btInstanceData.ActiveNodeId].NodeType == NodeType.Service)
                        {
                            BTServiceHelper.SetActiveService(ref entityManager, btInstanceData.ActiveNodeId, entity, false);
                        }
                        BTNodeResultHandler.ReturnNodeResult(ref btInstanceData, in btData);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            ecb.Playback(entityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}