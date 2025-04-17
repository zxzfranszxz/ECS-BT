using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Blackboard.Data
{
    public struct BBVarElement : IBufferElementData
    {
        public FixedString32Bytes Key;
        public unsafe fixed byte Value[32];
        public BBVarType VarType;
    }

    public enum BBVarType : byte
    {
        None = 0,
        Bool = 1,
        Int = 2,
        Float = 3,
        String = 4,
        Float3 = 5,
        Entity = 6,
    }
}