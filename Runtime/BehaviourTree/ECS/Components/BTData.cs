using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Components
{
    public struct BTData : IComponentData
    {
        public FixedString32Bytes BTName;
        public NativeArray<NodeData> Nodes;
    }
}