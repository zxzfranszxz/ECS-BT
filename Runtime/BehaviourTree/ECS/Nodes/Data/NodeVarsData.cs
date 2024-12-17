using Unity.Collections;
using Unity.Mathematics;

namespace Game.ECS.AI.BehaviourTree.Components.Nodes
{
    public struct NodeVarsData
    {
        public int NodeId;
        public NativeHashMap<FixedString32Bytes, bool> BoolVars;
        public NativeHashMap<FixedString32Bytes, int> IntVars;
        public NativeHashMap<FixedString32Bytes, float> FloatVars;
        public NativeHashMap<FixedString32Bytes, float2> Float2Vars;
        public NativeHashMap<FixedString32Bytes, float3> Float3Vars;
        public NativeHashMap<FixedString32Bytes, FixedString32Bytes> StringVars;
        // public NativeHashMap<FixedString32Bytes, AbortType> AbortVars;
        // public NativeHashMap<FixedString32Bytes, BlackboardConditionType> BlackboardConditionVars;
        // public NativeHashMap<FixedString32Bytes, NotifyType> NotifyVars;
    }
}