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
    [Name("Blackboard Bool Condition")]
    [Blackboard("Blackboard", typeof(bool))]
    [NodeVar("BoolCondition", typeof(bool))]
    [NodeVar("AbortType", typeof(AbortType))]
    [BurstCompile]
    public struct AIBlackboardBoolConditionNode : IDecoratorNode
    {
        [BurstCompile]
        [NodeHandler(typeof(AIBlackboardBoolConditionNode))]
        public static void Run(ref SystemState systemState, ref EntityCommandBuffer ecb, 
            ref BTInstanceData btInstanceData, ref Blackboard.BlackboardData blackboardData, in BTData btData, in Entity owner, 
            in Entity btInstance, in NodeData node, out ActiveNodeState activeNodeState)
        {
            var varId = node.StringVars["Blackboard"];
            var condition = node.BoolVars["BoolCondition"];
            if (blackboardData.BoolVars.TryGetValue(varId, out var value))
            {
                activeNodeState = value == condition ? ActiveNodeState.Success : ActiveNodeState.Failure;
            }
            else
            {
                activeNodeState = !condition ? ActiveNodeState.Success : ActiveNodeState.Failure;
            }
        }
    }
}