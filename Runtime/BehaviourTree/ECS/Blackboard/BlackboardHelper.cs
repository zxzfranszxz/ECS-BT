using SD.ECSBT.BehaviourTree.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Blackboard
{
    [BurstCompile]
    public static class BlackboardHelper
    {
        public static void CreateNotifyCommand(ref EntityCommandBuffer ecb,
            in FixedString32Bytes blackboardId, in NotifyType notifyType, in Entity btInstance)
        {
            var notifyCommand = ecb.CreateEntity();
            ecb.AddComponent(notifyCommand, new NotifyBlackboardVarCommand
            {
                BlackboardId = blackboardId,
                NotifyType = notifyType,
                BTInstance = btInstance,
            });
        }
    }
}