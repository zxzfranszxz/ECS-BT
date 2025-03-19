using Unity.Burst;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Components
{
    public struct BTDelegateData : IComponentData
    {
        public FunctionPointer<BTHelper.BTNodeReturnHandlerDelegate> BTNodeReturnHandlerFunc;
    }
}