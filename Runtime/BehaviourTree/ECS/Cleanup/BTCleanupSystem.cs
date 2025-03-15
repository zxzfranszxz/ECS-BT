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
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;
            foreach (var (aiControllerData, entity) in SystemAPI.Query<RefRO<AIControllerData>>()
                         .WithNone<AIControllerCleanupTag>().WithEntityAccess())
            {
                ecb.RemoveComponent<AIControllerData>(entity);
                var btInstance = aiControllerData.ValueRO.BTInstance;
                BTHelper.CleanupBTInstance(ref entityManager, ref ecb, btInstance);
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