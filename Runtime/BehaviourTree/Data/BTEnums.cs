using System;

namespace SD.ECSBT.BehaviourTree.Data
{
    [Flags]
    public enum AbortType
    {
        None = 0,
        Self = 0b01,
        LowerPriority = 0b10,
        Both = 0b11,
    }
    
    public enum NotifyType
    {
        None = 0,
        OnValueChange,
        OnValueExistStateChange
    }
    
    public enum BlackboardConditionType
    {
        IsSet = 0,
        IsNotSet
    }
}