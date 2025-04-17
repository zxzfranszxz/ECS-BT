using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Services
{
    public struct BTServiceData : IComponentData
    {
        public float Frequency;
        public float CurrentTime;

        public FixedString32Bytes BehaviorTree;
    }
}