using Unity.Collections;

namespace SD.ECSBT.BehaviourTree.ECS.Nodes.Data
{
    public struct ChildrenNodesData
    {
        public int NodeId;
        public NativeArray<int> ChildrenIds;
    }
}