using System.Runtime.CompilerServices;
using SD.ECSBT.BehaviourTree.Data;
using SD.ECSBT.BehaviourTree.ECS.Blackboard;
using SD.ECSBT.BehaviourTree.ECS.Blackboard.Data;
using SD.ECSBT.BehaviourTree.ECS.Components;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace SD.ECSBT.BehaviourTree.ECS.Instance
{
    public readonly partial struct BTInstanceAspect : IAspect
    {
        public readonly Entity Entity;

        public readonly RefRW<BTInstanceData> InstanceDataRW;
        public readonly RefRO<BTOwner> OwnerRO;

        public readonly DynamicBuffer<BBVarElement> BBVarBuffer;

        // props
        public FixedString32Bytes BehaviorTree => InstanceDataRW.ValueRO.BehaviorTree;
        public Entity Owner => OwnerRO.ValueRO.Value;

        public BTData GetBTData(in BTDataElements btDataElements)
        {
            if(btDataElements.BTDataMap.IsCreated)
                return btDataElements.BTDataMap[BehaviorTree];
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBBSet(FixedString32Bytes varId)
        {
            foreach (var element in BBVarBuffer)
            {
                if (element.Key != varId) continue;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T GetBB<T>(FixedString32Bytes varId) where T : struct
        {
            foreach (var element in BBVarBuffer)
            {
                if (element.Key != varId) continue;
                var v = element.Value;
                return UnsafeUtility.AsRef<T>(v);
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool TryGetBB<T>(FixedString32Bytes key, out T value) where T : struct
        {
            foreach (var element in BBVarBuffer)
            {
                if (element.Key != key) continue;
                var v = element.Value;
                value = UnsafeUtility.AsRef<T>(v);
                return true;
            }

            value = default;
            return false;
        }

        public unsafe void SetBB<T>(FixedString32Bytes key, T value) where T : struct
        {
            var buffer = BBVarBuffer;
            var hasValue = false;
            for (var i = 0; i < buffer.Length; i++)
            {
                var element = buffer[i];
                if (element.Key != key) continue;
                void* dst = element.Value;
                UnsafeUtility.CopyStructureToPtr(ref value, dst);
                buffer[i] = element;
                hasValue = true;
            }

            if (hasValue == false)
            {
                var element = new BBVarElement { Key = key };
                void* dst = element.Value;
                UnsafeUtility.CopyStructureToPtr(ref value, dst);
                buffer.Add(element);
            }
        }

        public unsafe void SetBBNotify<T>(ref EntityCommandBuffer ecb, string key, T value,
            BBVarType varType = BBVarType.None) where T : struct
        {
            const float eps = 1e-5f;

            var notifyType = NotifyType.None;
            var hasValue = false;
            var buffer = BBVarBuffer;
            for (var i = 0; i < buffer.Length; i++)
            {
                var element = buffer[i];
                if (element.Key != key) continue;

                // compare common
                if (varType == BBVarType.None)
                {
                    var ptrA = (void*)element.Value;
                    var ptrB = UnsafeUtility.AddressOf(ref value);
                    var size = UnsafeUtility.SizeOf<T>();
                    if (UnsafeUtility.MemCmp(ptrA, ptrB, size) != 0)
                        notifyType = NotifyType.OnValueChange;
                }
                else if (varType == BBVarType.Float)
                {
                    ref var fa = ref UnsafeUtility.AsRef<float>(element.Value);
                    ref var fb = ref UnsafeUtility.As<T, float>(ref value);
                    if (math.abs(fa - fb) > eps)
                        notifyType = NotifyType.OnValueChange;
                }
                else if (varType == BBVarType.Float3)
                {
                    ref var f3A = ref UnsafeUtility.AsRef<float3>(element.Value);
                    ref var f3B = ref UnsafeUtility.As<T, float3>(ref value);
                    if (math.any(math.abs(f3A - f3B) > eps))
                        notifyType = NotifyType.OnValueChange;
                }

                void* dst = element.Value;
                UnsafeUtility.CopyStructureToPtr(ref value, dst);
                buffer[i] = element;
                hasValue = true;
                break;
            }

            if (hasValue == false)
            {
                notifyType = NotifyType.OnValueExistStateChange;
                var element = new BBVarElement { Key = key };
                void* dst = element.Value;
                UnsafeUtility.CopyStructureToPtr(ref value, dst);
                buffer.Add(element);
            }

            BlackboardHelper.CreateNotifyCommand(ref ecb, key, notifyType, Entity);
        }
    }
}