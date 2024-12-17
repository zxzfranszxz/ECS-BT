using Game.ECS.AI.BehaviourTree.Components.Nodes;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Components
{
    public struct BTData : IComponentData
    {
        public FixedString32Bytes BTName;
        public NativeArray<NodeData> Nodes;
        public NativeHashMap<int, NodeVarsData> NodeVars;
    }

    public delegate void RunDelegate(ref SystemState state, ref EntityCommandBuffer ecb);
}