using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS
{
    public struct AIControllerData : IComponentData
    {
        public Entity BTInstance;
    }
}