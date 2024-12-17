using System;
using SD.ECSBT.BehaviourTree.Nodes;
using Unity.Collections;
using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Components.Nodes
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