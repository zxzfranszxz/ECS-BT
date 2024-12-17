using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Services
{
    public struct BTServiceElement : IBufferElementData
    {
        public int NodeId;
        public Entity Service;
    }
}