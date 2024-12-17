using Unity.Collections;
using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Setup
{
    public struct BTSetCommand : IComponentData
    {
        public Entity Target;
        public FixedString32Bytes BTName;
    }
}