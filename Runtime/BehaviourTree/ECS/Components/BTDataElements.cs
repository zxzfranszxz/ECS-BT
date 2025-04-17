using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Components
{
    public struct BTDataElements : IComponentData
    {
        public NativeHashMap<FixedString32Bytes, BTData> BTDataMap;
    }
}