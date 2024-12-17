using Unity.Entities;

namespace Game.ECS.AI
{
    public struct AIControllerLink : IComponentData
    {
        public Entity Controller;
    }
}