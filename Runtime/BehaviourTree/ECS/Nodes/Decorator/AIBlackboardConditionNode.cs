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
            ref BTInstanceData btInstanceData, ref Blackboard.BlackboardData blackboardData, in BTData btData, in Entity owner, 
            in Entity btInstance, in NodeData node, out ActiveNodeState activeNodeState)
        {
            var varId = node.StringVars["Blackboard"];
            var condition = (BlackboardConditionType)node.IntVars["ConditionType"];
            var isSet = blackboardData.BoolVars.ContainsKey(varId) ||
                        blackboardData.IntVars.ContainsKey(varId) ||
                        blackboardData.FloatVars.ContainsKey(varId) ||
                        blackboardData.StringVars.ContainsKey(varId) ||
                        blackboardData.Float2Vars.ContainsKey(varId) ||
                        blackboardData.Float3Vars.ContainsKey(varId) ||
                        blackboardData.EntityVars.ContainsKey(varId) ||
                        blackboardData.QuaternionVars.ContainsKey(varId);

            if (isSet && blackboardData.EntityVars.TryGetValue(varId, out var entity))
            {
                if(!systemState.EntityManager.Exists(entity))
                {
                    blackboardData.EntityVars.Remove(varId);
                    isSet = false;
                }
            }
            
            activeNodeState = condition switch
            {
                BlackboardConditionType.IsSet => isSet ? ActiveNodeState.Success : ActiveNodeState.Failure,
                BlackboardConditionType.IsNotSet => !isSet ? ActiveNodeState.Success : ActiveNodeState.Failure,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}