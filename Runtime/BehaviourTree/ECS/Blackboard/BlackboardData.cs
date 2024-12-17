using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SD.ECSBT.BehaviourTree.ECS.Blackboard
{
    public struct BlackboardData : IComponentData
    {
        public NativeHashMap<FixedString32Bytes, bool> BoolVars;
        public NativeHashMap<FixedString32Bytes, int> IntVars;
        public NativeHashMap<FixedString32Bytes, float> FloatVars;
        public NativeHashMap<FixedString32Bytes, float2> Float2Vars;
        public NativeHashMap<FixedString32Bytes, float3> Float3Vars;
        public NativeHashMap<FixedString32Bytes, quaternion> QuaternionVars;
        public NativeHashMap<FixedString32Bytes, Entity> EntityVars;
        public NativeHashMap<FixedString32Bytes, FixedString32Bytes> StringVars;
    }
}