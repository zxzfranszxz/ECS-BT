using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS
{
    public struct AIControllerLink : ICleanupComponentData
    {
        public Entity Controller;
    }
    
    public struct AIControllerCleanupTag : IComponentData { }
}