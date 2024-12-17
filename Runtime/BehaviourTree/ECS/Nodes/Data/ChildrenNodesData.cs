using Unity.Collections;

namespace Game.ECS.AI.BehaviourTree.Components.Nodes
{
    public struct ChildrenNodesData
    {
        public int NodeId;
        public NativeArray<int> ChildrenIds;
    }
}