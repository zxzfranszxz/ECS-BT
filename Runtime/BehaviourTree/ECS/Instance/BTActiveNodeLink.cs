using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Instance
{
    public struct BTActiveNodeLink : IComponentData
    {
        public Entity ActiveNode;
    }
}