using SD.ECSBT.BehaviourTree.ECS.Blackboard;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Cleanup
{
    public partial struct BTCleanupSystem : ISystem
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
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;
            foreach (var (aiControllerData, entity) in SystemAPI.Query<RefRO<AIControllerData>>()
                         .WithNone<AIControllerCleanupTag>().WithEntityAccess())
            {
                ecb.RemoveComponent<AIControllerData>(entity);
                var btInstance = aiControllerData.ValueRO.BTInstance;
                
                ref var btInstanceData = ref SystemAPI.GetComponentRW<BTInstanceData>(btInstance).ValueRW;
                ref var blackboard = ref SystemAPI.GetComponentRW<BlackboardData>(btInstance).ValueRW;
                ref readonly var oldBTData = ref SystemAPI.GetComponentRO<BTData>(btInstanceData.BehaviorTree).ValueRO; 
                BTHelper.CleanupBTInstance(ref state, ref entityManager, ref ecb, ref btInstanceData,
                        ref blackboard, oldBTData, entity, btInstance, btDelegateData);
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