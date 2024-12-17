using Unity.Burst;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Services
{
    [BurstCompile]
    public static class BTServiceHelper
    {
        [BurstCompile]
        public static void SetActiveService(ref EntityManager entityManager, in int nodeId, in Entity btInstance, in bool state)
        {
            var serviceElements = entityManager.GetBuffer<BTServiceElement>(btInstance);
            for (var i = 0; i < serviceElements.Length; i++)
            {
                if(serviceElements[i].NodeId != nodeId) continue;
                entityManager.SetComponentEnabled<BTServiceEnabled>(serviceElements[i].Service, state);
                break;
            }
        }

        public static void DeactivateServicesBelowNode(ref EntityManager entityManager, in int nodeId, in Entity btInstance)
        {
            var serviceElements = entityManager.GetBuffer<BTServiceElement>(btInstance);
            for (var i = 0; i < serviceElements.Length; i++)
            {
                if(serviceElements[i].NodeId < nodeId) continue;
                entityManager.SetComponentEnabled<BTServiceEnabled>(serviceElements[i].Service, false);
            }
        }
    }
}