using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Instance
{
    public struct BTInstanceLink : IComponentData
    {
        public Entity BTInstance;
    }
}