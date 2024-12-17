using Unity.Collections;
using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Components
{
    public struct BTElement : IBufferElementData
    {
        public FixedString32Bytes Name;
        public Entity BTEntity;
    }
}