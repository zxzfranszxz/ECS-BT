using SD.ECSBT.BehaviourTree.Data;
using Unity.Collections;
using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Blackboard
{
    public struct NotifyBlackboardVarCommand : IComponentData
    {
        public FixedString32Bytes BlackboardId;
        public NotifyType NotifyType;
        public Entity BTInstance;
    }
}