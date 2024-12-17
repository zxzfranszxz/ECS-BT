using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS
{
    public struct AIControllerLink : IComponentData
    {
        public Entity Controller;
    }
}