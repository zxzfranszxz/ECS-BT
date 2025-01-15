using System;
using SD.ECSBT.BehaviourTree.Data;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.Nodes.Attributes;
using Unity.Burst;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Nodes.Decorator
{
    [Name("Blackboard Condition")]
    [Blackboard("Blackboard", typeof(object))]
    [NodeVar("ConditionType", typeof(BlackboardConditionType))]
    [NodeVar("NotifyType", typeof(NotifyType))]
    [NodeVar("AbortType", typeof(AbortType))]
    [BurstCompile]
    public struct AIBlackboardAbortNode : IComponentData
    {
        [BurstCompile]
        [NodeHandler(typeof(AIBlackboardAbortNode))]
        public static void Run(ref SystemState systemState, ref EntityCommandBuffer ecb, 
            ref BTInstanceData btInstanceData, ref Blackboard.BlackboardData blackboardData, in BTData btData, in Entity owner, 
            in Entity btInstance, in NodeData node, out ActiveNodeState activeNodeState)
        {
            activeNodeState = ActiveNodeState.Success;
        }
    }
}