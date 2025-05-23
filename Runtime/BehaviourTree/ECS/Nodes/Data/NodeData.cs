using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SD.ECSBT.BehaviourTree.ECS.Nodes.Data
{
    public struct NodeData
    {
        public NodeType NodeType;
        public ComponentType NodeComponentType;
        public ulong StableTypeHash;
        public Guid Guid;
        public int Id;
        public int ParentId;
        public NativeArray<int> Children;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsAutoReturn;
        
        // vars
        public NativeHashMap<FixedString32Bytes, bool> BoolVars;
        public NativeHashMap<FixedString32Bytes, int> IntVars;
        public NativeHashMap<FixedString32Bytes, float> FloatVars;
        public NativeHashMap<FixedString32Bytes, float2> Float2Vars;
        public NativeHashMap<FixedString32Bytes, float3> Float3Vars;
        public NativeHashMap<FixedString32Bytes, FixedString32Bytes> StringVars;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedString32Bytes GetBlackboardId(FixedString32Bytes key)
        {
            return StringVars[key];
        }
    }
}