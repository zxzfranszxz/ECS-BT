using SD.ECSBT.BehaviourTree.Data;
using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Blackboard
{
    public struct AbortSubscriberElement : IBufferElementData
    {
        public NotifyType NotifyType;
        public AbortType AbortType;
        public int NodeId;
        public FixedString32Bytes BlackboardId;
    }
}