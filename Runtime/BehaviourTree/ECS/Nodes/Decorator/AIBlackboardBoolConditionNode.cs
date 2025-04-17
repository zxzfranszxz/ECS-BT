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
            in BTInstanceAspect btInstance, in BTData btData, in NodeData node, out ActiveNodeState activeNodeState)
        {
            var varId = node.StringVars["Blackboard"];
            var condition = node.BoolVars["BoolCondition"];
            
            var result = !condition;
            if (btInstance.TryGetBB<bool>(varId, out var value))
            {
                result = value == condition;
            }
            
            activeNodeState = result ? ActiveNodeState.Success : ActiveNodeState.Failure;
        }
    }
}