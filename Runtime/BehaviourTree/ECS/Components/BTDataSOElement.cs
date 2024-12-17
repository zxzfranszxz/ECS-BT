using SD.ECSBT.BehaviourTree;
using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Components
{
    public struct BTDataSOElement : IBufferElementData
    {
        public UnityObjectRef<BehaviourTreeSO> BTData;
    }
}