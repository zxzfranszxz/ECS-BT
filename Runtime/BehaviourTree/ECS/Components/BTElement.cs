using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Components
{
    public struct BTElement : IBufferElementData
    {
        public FixedString32Bytes Name;
        public Entity BTEntity;
    }
}