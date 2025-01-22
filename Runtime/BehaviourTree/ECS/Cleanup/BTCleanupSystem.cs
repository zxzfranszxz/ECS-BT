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
            foreach (var (aiControllerLink, entity) in SystemAPI.Query<RefRO<AIControllerLink>>()
                         .WithNone<AIControllerCleanupTag>().WithEntityAccess())
            {
                ecb.RemoveComponent<AIControllerLink>(entity);
                if (!SystemAPI.Exists(aiControllerLink.ValueRO.Controller)) continue;
                var btInstance = SystemAPI.GetComponentRO<AIControllerData>(aiControllerLink.ValueRO.Controller).ValueRO
                    .BTInstance;
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