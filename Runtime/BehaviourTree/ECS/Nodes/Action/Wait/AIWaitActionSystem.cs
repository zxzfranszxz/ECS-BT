using SD.ECSBT.BehaviourTree.ECS.Instance;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Nodes.Action.Wait
{
    [UpdateInGroup(typeof(BTActionsSystemGroup))]
    public partial struct AIWaitActionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (aiWaitNode, nodeData, entity) in SystemAPI.Query<RefRW<AIWaitNode>, RefRO<NodeInstanceData>>().WithEntityAccess())
            {
                aiWaitNode.ValueRW.LeftTime -= SystemAPI.Time.DeltaTime;
                if (aiWaitNode.ValueRO.LeftTime > 0) continue;
                
                var entityManager = state.EntityManager;
                BTHelper.FinishAction(ref entityManager, ref ecb, in entity, in nodeData.ValueRO.BTInstance,
                    ActiveNodeState.Success);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}