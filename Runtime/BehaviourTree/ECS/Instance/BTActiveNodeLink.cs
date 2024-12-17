using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Instance
{
    public struct BTActiveNodeLink : IComponentData
    {
        public Entity ActiveNode;
    }
}