using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS
{
    public struct AIControllerData : ICleanupComponentData
    {
        public Entity BTInstance;
    }
    
    public struct AIControllerCleanupTag : IComponentData { }
}