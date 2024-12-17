using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Components
{
    public struct BTOwner : IComponentData
    {
        public Entity Value;
    }
}