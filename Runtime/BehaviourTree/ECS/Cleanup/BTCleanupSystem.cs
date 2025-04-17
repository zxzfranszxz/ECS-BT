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
            state.RequireForUpdate<BTDataElements>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var btDelegateData = SystemAPI.GetSingleton<BTDelegateData>();
            var btDataElements = SystemAPI.GetSingleton<BTDataElements>();
            
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;
            foreach (var (aiControllerData, entity) in SystemAPI.Query<RefRO<AIControllerData>>()
                         .WithNone<AIControllerCleanupTag>().WithEntityAccess())
            {
                ecb.RemoveComponent<AIControllerData>(entity);
                var btInstanceEntity = aiControllerData.ValueRO.BTInstance;

                var btInstance = SystemAPI.GetAspect<BTInstanceAspect>(btInstanceEntity);
                var btData = btInstance.GetBTData(btDataElements);
                BTHelper.CleanupBTInstance(ref state, ref entityManager, ref ecb, btInstance, btData, btDelegateData);
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