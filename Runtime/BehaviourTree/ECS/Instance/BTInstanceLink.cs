using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Instance
{
    public struct BTInstanceLink : IComponentData
    {
        public Entity BTInstance;
    }
}