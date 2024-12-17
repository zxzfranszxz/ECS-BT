using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Components
{
    public struct BTOwner : IComponentData
    {
        public Entity Value;
    }
}