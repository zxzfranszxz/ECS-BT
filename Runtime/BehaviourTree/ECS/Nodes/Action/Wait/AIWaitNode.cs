using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.Nodes;
using SD.ECSBT.BehaviourTree.Nodes.Attributes;
using Unity.Burst;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Nodes.Action.Wait
{
    [Name("Wait")]
    [NodeVar("Time", typeof(float))]
    [BurstCompile]
    public struct AIWaitNode : IActionNode
    {
        public float LeftTime;

        [BurstCompile]
        [NodeHandler(typeof(AIWaitNode))]
        public static void Run(ref SystemState systemState, ref EntityCommandBuffer ecb, 
            ref BTInstanceData btInstanceData, ref Blackboard.BlackboardData blackboardData, in BTData btData, in Entity owner, 
            in Entity btInstance, in NodeData node, out ActiveNodeState activeNodeState)
        {
            var action = BTHelper.CreateAction(ref ecb, "AIWaitAction", btInstance, owner, node);
            
            ecb.AddComponent(action, new AIWaitNode
            {
                LeftTime = node.FloatVars["Time"]
            });

            activeNodeState = ActiveNodeState.Running;
        }
    }
}