using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.Nodes
{
    public interface IDecoratorNode : INode, IComponentData
    {
    }
}