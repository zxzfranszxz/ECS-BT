using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Instance
{
    public struct BTInstanceData : IComponentData
    {
        public FixedString32Bytes BehaviorTree;
        public int ActiveNodeId;
        public int PreviousNodeId;
        public ActiveNodeState ActiveNodeState;
        public BTRunState RunState;

        public static void Reset(ref BTInstanceData btInstanceData)
        {
            btInstanceData.RunState = BTRunState.None;
            btInstanceData.ActiveNodeId = 0;
            btInstanceData.PreviousNodeId = 0;
            btInstanceData.ActiveNodeState = ActiveNodeState.None;
        }
    }
}