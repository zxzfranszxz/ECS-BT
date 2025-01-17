using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Nodes
{
    public struct NodeInstanceData : IComponentData
    {
        public Entity BTInstance;
        public Entity BTOwner;
        public int NodeId;
    }
}