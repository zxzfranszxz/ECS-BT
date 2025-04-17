using System;
using SD.ECSBT.BehaviourTree.Data;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.Nodes;
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
    public struct AIBlackboardConditionNode : IDecoratorNode
    {
        [BurstCompile]
        [NodeHandler(typeof(AIBlackboardConditionNode))]
        public static void Run(ref SystemState systemState, ref EntityCommandBuffer ecb, 
            in BTInstanceAspect btInstance, in BTData btData, in NodeData node, out ActiveNodeState activeNodeState)
        {
            var varId = node.StringVars["Blackboard"];
            var condition = (BlackboardConditionType)node.IntVars["ConditionType"];

            var isSet = btInstance.IsBBSet(varId);
            
            activeNodeState = condition switch
            {
                BlackboardConditionType.IsSet => isSet ? ActiveNodeState.Success : ActiveNodeState.Failure,
                BlackboardConditionType.IsNotSet => !isSet ? ActiveNodeState.Success : ActiveNodeState.Failure,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}