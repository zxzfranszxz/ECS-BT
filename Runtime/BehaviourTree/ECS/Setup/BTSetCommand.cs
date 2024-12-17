using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Setup
{
    public struct BTSetCommand : IComponentData
    {
        public Entity Target;
        public FixedString32Bytes BTName;
    }
}