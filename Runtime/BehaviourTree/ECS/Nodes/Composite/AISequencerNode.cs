using SD.ECSBT.BehaviourTree.Nodes;
using SD.ECSBT.BehaviourTree.Nodes.Attributes;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Nodes.Composite
{
    [Name("Sequencer")]
    public struct AISequencerNode : IComponentData, ICompositeNode
    {
    }
}