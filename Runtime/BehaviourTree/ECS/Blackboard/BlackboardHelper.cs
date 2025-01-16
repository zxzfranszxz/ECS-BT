using SD.ECSBT.BehaviourTree.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SD.ECSBT.BehaviourTree.ECS.Blackboard
{
    [BurstCompile]
    public static class BlackboardHelper
    {
        // Entity
        [BurstCompile]
        public static void UpdateVar(ref BlackboardData blackboardData, ref EntityCommandBuffer ecb,
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
        
        // quaternion
        [BurstCompile]
        public static void UpdateVar(ref BlackboardData blackboardData, ref EntityCommandBuffer ecb,
            in FixedString32Bytes blackboardId, in quaternion value, in Entity btInstance)
        {
            var shouldNotify = false;
            var notifyType = NotifyType.OnValueChange;
            if (blackboardData.QuaternionVars.TryGetValue(blackboardId, out var currentValue))
            {
                if (math.any(currentValue.value != value.value))
                {
                    blackboardData.QuaternionVars[blackboardId] = value;
                    shouldNotify = true;
                }
            }
            else
            {
                notifyType = NotifyType.OnValueExistStateChange;
                blackboardData.QuaternionVars.Add(blackboardId, value);
                shouldNotify = true;
            }
            
            if (!shouldNotify) return;
            CreateNotifyCommand(ref ecb, blackboardId, notifyType, btInstance);
        }
        
        // string
        [BurstCompile]
        public static void UpdateVar(ref BlackboardData blackboardData, ref EntityCommandBuffer ecb,
            in FixedString32Bytes blackboardId, in FixedString32Bytes value, in Entity btInstance)
        {
            var shouldNotify = false;
            var notifyType = NotifyType.OnValueChange;
            if (blackboardData.StringVars.TryGetValue(blackboardId, out var currentValue))
            {
                if (currentValue != value)
                {
                    blackboardData.StringVars[blackboardId] = value;
                    shouldNotify = true;
                }
            }
            else
            {
                notifyType = NotifyType.OnValueExistStateChange;
                blackboardData.StringVars.Add(blackboardId, value);
                shouldNotify = true;
            }
            
            if (!shouldNotify) return;
            CreateNotifyCommand(ref ecb, blackboardId, notifyType, btInstance);
        }

        // bool
        [BurstCompile]
        public static void UpdateVar(ref BlackboardData blackboardData, ref EntityCommandBuffer ecb,
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
        
        // int
        [BurstCompile]
        public static void UpdateVar(ref BlackboardData blackboardData, ref EntityCommandBuffer ecb,
            in FixedString32Bytes blackboardId, in int value, in Entity btInstance)
        {
            var shouldNotify = false;
            var notifyType = NotifyType.OnValueChange;
            if (blackboardData.IntVars.TryGetValue(blackboardId, out var currentValue))
            {
                if (!Mathf.Approximately(currentValue, value))
                {
                    blackboardData.IntVars[blackboardId] = value;
                    shouldNotify = true;
                }
            }
            else
            {
                notifyType = NotifyType.OnValueExistStateChange;
                blackboardData.IntVars.Add(blackboardId, value);
                shouldNotify = true;
            }
            
            if (!shouldNotify) return;
            CreateNotifyCommand(ref ecb, blackboardId, notifyType, btInstance);
        }
        
        // float
        [BurstCompile]
        public static void UpdateVar(ref BlackboardData blackboardData, ref EntityCommandBuffer ecb,
            in FixedString32Bytes blackboardId, in float value, in Entity btInstance)
        {
            var shouldNotify = false;
            var notifyType = NotifyType.OnValueChange;
            if (blackboardData.FloatVars.TryGetValue(blackboardId, out var currentValue))
            {
                if (!Mathf.Approximately(currentValue, value))
                {
                    blackboardData.FloatVars[blackboardId] = value;
                    shouldNotify = true;
                }
            }
            else
            {
                notifyType = NotifyType.OnValueExistStateChange;
                blackboardData.FloatVars.Add(blackboardId, value);
                shouldNotify = true;
            }
            
            if (!shouldNotify) return;
            CreateNotifyCommand(ref ecb, blackboardId, notifyType, btInstance);
        }
        
        // float2
        [BurstCompile]
        public static void UpdateVar(ref BlackboardData blackboardData, ref EntityCommandBuffer ecb, in FixedString32Bytes blackboardId, in float2 value, in Entity btInstance)
        {
            var shouldNotify = false;
            var notifyType = NotifyType.OnValueChange;
            if (blackboardData.Float2Vars.TryGetValue(blackboardId, out var currentValue))
            {
                if (math.any(currentValue != value))
                {
                    blackboardData.Float2Vars[blackboardId] = value;
                    shouldNotify = true;
                }
            }
            else
            {
                notifyType = NotifyType.OnValueExistStateChange;
                blackboardData.Float2Vars.Add(blackboardId, value);
                shouldNotify = true;
            }
            
            if (!shouldNotify) return;
            CreateNotifyCommand(ref ecb, blackboardId, notifyType, btInstance);
        }
        
        // float3
        [BurstCompile]
        public static void UpdateVar(ref BlackboardData blackboardData, ref EntityCommandBuffer ecb, in FixedString32Bytes blackboardId, in float3 value, in Entity btInstance)
        {
            var shouldNotify = false;
            var notifyType = NotifyType.OnValueChange;
            if (blackboardData.Float3Vars.TryGetValue(blackboardId, out var currentValue))
            {
                if (math.any(currentValue != value))
                {
                    blackboardData.Float3Vars[blackboardId] = value;
                    shouldNotify = true;
                }
            }
            else
            {
                notifyType = NotifyType.OnValueExistStateChange;
                blackboardData.Float3Vars.Add(blackboardId, value);
                shouldNotify = true;
            }
            
            if (!shouldNotify) return;
            CreateNotifyCommand(ref ecb, blackboardId, notifyType, btInstance);
        }

        [BurstCompile]
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