using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Components
{
    public struct BTDataSOElement : IBufferElementData
    {
        public UnityObjectRef<BehaviourTreeSO> BTData;
    }
}