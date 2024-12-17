using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Services
{
    public struct BTServiceElement : IBufferElementData
    {
        public int NodeId;
        public Entity Service;
    }
}