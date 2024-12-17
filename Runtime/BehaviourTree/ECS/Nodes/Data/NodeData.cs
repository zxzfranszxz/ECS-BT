using System;
using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Nodes.Data
{
    public struct NodeData
    {
        public NodeType NodeType;
        public ComponentType NodeComponentType;
        public Guid Guid;
        public int Id;
        public int ParentId;
        public NativeArray<int> Children;
    }
}