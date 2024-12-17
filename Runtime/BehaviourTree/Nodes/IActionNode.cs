using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.Nodes
{
    public interface IActionNode : INode, IComponentData, IEnableableComponent
    {
    }
}