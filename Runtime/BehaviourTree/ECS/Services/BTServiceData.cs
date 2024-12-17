using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Services
{
    public struct BTServiceData : IComponentData
    {
        public float Frequency;
        public float CurrentTime;
        public Entity BTEntity;
    }
}