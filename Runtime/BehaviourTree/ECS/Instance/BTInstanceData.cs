using Unity.Entities;

namespace Game.ECS.AI.BehaviourTree.Instance
{
    public struct BTInstanceData : IComponentData
    {
        public Entity BehaviorTree;
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