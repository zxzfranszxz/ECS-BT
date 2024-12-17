using SD.ECSBT.BehaviourTree.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Blackboard
{
    [BurstCompile]
    public static class BlackboardHelper
    {
        [BurstCompile]
        public static void TryUpdateVar(ref BlackboardData blackboardData, ref EntityCommandBuffer ecb,
            in FixedString32Bytes blackboardId, in Entity value, in Entity btInstance)
        {
            var shouldNotify = false;
            var notifyType = NotifyType.OnValueChange;
            if (blackboardData.EntityVars.TryGetValue(blackboardId, out var currentValue))
            {
                if (currentValue != value)
                {
                    blackboardData.EntityVars[blackboardId] = value;
                    shouldNotify = true;
                }
            }
            else
            {
                notifyType = NotifyType.OnValueExistStateChange;
                blackboardData.EntityVars.Add(blackboardId, value);
                shouldNotify = true;
            }
            
            if (!shouldNotify) return;
            CreateNotifyCommand(ref ecb, blackboardId, notifyType, btInstance);
        }

        [BurstCompile]
        public static void TryUpdateVar(ref BlackboardData blackboardData, ref EntityCommandBuffer ecb,
            in FixedString32Bytes blackboardId, in bool value, in Entity btInstance)
        {
            var shouldNotify = false;
            var notifyType = NotifyType.OnValueChange;
            if (blackboardData.BoolVars.TryGetValue(blackboardId, out var currentValue))
            {
                if (currentValue != value)
                {
                    blackboardData.BoolVars[blackboardId] = value;
                    shouldNotify = true;
                }
            }
            else
            {
                notifyType = NotifyType.OnValueExistStateChange;
                blackboardData.BoolVars.Add(blackboardId, value);
                shouldNotify = true;
            }
            
            if (!shouldNotify) return;
            CreateNotifyCommand(ref ecb, blackboardId, notifyType, btInstance);
        }

        private static void CreateNotifyCommand(ref EntityCommandBuffer ecb,
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