using System;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Composite;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Decorator;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace SD.ECSBT.BehaviourTree.ECS.Process
{
    [BurstCompile]
    public static class BTNodeResultHandler
    {
        [BurstCompile]
        public static void ReturnNodeResult(ref BTInstanceData btInstanceData, in BTData btData, in DynamicBuffer<BTActiveAutoReturnNodeElement> autoReturnNods)
        {
            var node = btData.Nodes[btInstanceData.ActiveNodeId];
            if (btInstanceData.RunState == BTRunState.Digging)
            {
                if (node.Children is { IsCreated: true, Length: > 0 } && btInstanceData.ActiveNodeState == ActiveNodeState.Success)
                {
                    btInstanceData.PreviousNodeId = btInstanceData.ActiveNodeId;
                    btInstanceData.ActiveNodeId = node.Children[0];
                    btInstanceData.ActiveNodeState = ActiveNodeState.None;
                    // add auto return nodes
                    if (node.IsAutoReturn)
                    {
                        autoReturnNods.Add(new BTActiveAutoReturnNodeElement { NodeId = node.Id });
                    }
                }
                else
                {
                    btInstanceData.PreviousNodeId = btInstanceData.ActiveNodeId;
                    btInstanceData.ActiveNodeId = node.ParentId;
                    btInstanceData.RunState = BTRunState.Returning;
                }
            }
            else if (btInstanceData.RunState == BTRunState.Returning)
            {
                if (node.Id == 0)
                {
                    BTInstanceData.Reset(ref btInstanceData);
                }
                else if (node.NodeType == NodeType.Composite)
                {
                    var isSelector = node.NodeComponentType == ComponentType.ReadWrite<AISelectorNode>();
                    var isSequencer = node.NodeComponentType == ComponentType.ReadWrite<AISequencerNode>();

                    if ((btInstanceData.ActiveNodeState == ActiveNodeState.Failure && isSelector) ||
                        (btInstanceData.ActiveNodeState == ActiveNodeState.Success && isSequencer))
                    {
                        var childIndex = node.Children.IndexOf(btInstanceData.PreviousNodeId);
                        if (childIndex < node.Children.Length - 1)
                        {
                            btInstanceData.PreviousNodeId = btInstanceData.ActiveNodeId;
                            btInstanceData.ActiveNodeId = node.Children[childIndex + 1];
                            btInstanceData.RunState = BTRunState.Digging;
                            btInstanceData.ActiveNodeState = ActiveNodeState.None;
                        }
                        else
                        {
                            btInstanceData.PreviousNodeId = btInstanceData.ActiveNodeId;
                            btInstanceData.ActiveNodeId = node.ParentId;
                        }
                    }
                    else if ((btInstanceData.ActiveNodeState == ActiveNodeState.Success && isSelector) ||
                             (btInstanceData.ActiveNodeState == ActiveNodeState.Failure && isSequencer))
                    {
                        btInstanceData.PreviousNodeId = btInstanceData.ActiveNodeId;
                        btInstanceData.ActiveNodeId = node.ParentId;
                    }
                    else
                    {
                        throw new Exception($"Can't return result node with state: {btInstanceData.ActiveNodeState}");
                    }
                }
                else
                {
                    btInstanceData.PreviousNodeId = btInstanceData.ActiveNodeId;
                    btInstanceData.ActiveNodeId = node.ParentId;
                    
                    // add auto return nodes
                    if (node.IsAutoReturn)
                    {
                        var index = -1;
                        for (var i = 0; i < autoReturnNods.Length; i++)
                        {
                            if(autoReturnNods[i].NodeId != node.Id) continue;
                            index = i;
                            break;
                        }

                        if (index >= 0)
                        {
                            autoReturnNods.RemoveAt(index);
                        }
                    }
                }
            }
            else
            {
                throw new Exception($"Can't handle BTRunState:{btInstanceData.RunState}");
            }
        }
    }
}