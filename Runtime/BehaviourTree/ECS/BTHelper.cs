using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SD.ECSBT.BehaviourTree.ECS.Blackboard;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Decorator;
using SD.ECSBT.BehaviourTree.ECS.Services;
using SD.ECSBT.BehaviourTree.Nodes;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace SD.ECSBT.BehaviourTree.ECS
{
    [BurstCompile]
    public static class BTHelper
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void BTNodeReturnHandlerDelegate(ref SystemState systemState, ref EntityCommandBuffer ecb,
            ref BTInstanceData btInstanceData, ref BlackboardData blackboardData, in BTData btData, in Entity owner,
            in Entity btInstance, in int nodeId);

        public static NodeType GetNodeType(Type type)
        {
            if (typeof(IRootNode).IsAssignableFrom(type))
                return NodeType.Root;
            if (typeof(ICompositeNode).IsAssignableFrom(type))
                return NodeType.Composite;
            if (typeof(IDecoratorNode).IsAssignableFrom(type))
                return NodeType.Decorator;
            if (typeof(IActionNode).IsAssignableFrom(type))
                return NodeType.Action;
            if (typeof(IServiceNode).IsAssignableFrom(type))
                return NodeType.Service;
            return NodeType.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity CreateAction(ref EntityCommandBuffer ecb, in FixedString32Bytes actionName,
            in Entity btInstance, in Entity owner, in NodeData nodeData)
        {
            var action = ecb.CreateEntity();
            ecb.SetName(action, actionName);
            ecb.AddComponent(action, new NodeInstanceData
            {
                BTInstance = btInstance,
                NodeId = nodeData.Id,
                BTOwner = owner
            });
            ecb.AddComponent(action, nodeData.NodeComponentType);
            ecb.AddComponent(btInstance, new BTActiveNodeLink { ActiveNode = action });

            return action;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FinishAction(ref EntityManager entityManager, ref EntityCommandBuffer ecb,
            in Entity actionEntity, in Entity btInstance, in ActiveNodeState activeNodeState)
        {
            ecb.DestroyEntity(actionEntity);
            var btInstanceData = entityManager.GetComponentData<BTInstanceData>(btInstance);
            if (btInstanceData.ActiveNodeState == ActiveNodeState.Running)
            {
                btInstanceData.ActiveNodeState = activeNodeState;
                btInstanceData.RunState = BTRunState.Returning;
                ecb.RemoveComponent<BTActiveNodeLink>(btInstance);
                entityManager.SetComponentEnabled<BTLogicEnabled>(btInstance, true);
                entityManager.SetComponentData(btInstance, btInstanceData);
            }
            else
            {
                Debug.LogError("Unexpected issue in AIWaitActionSystem");
            }
        }

        [BurstCompile]
        public static void CleanupBTInstance(ref SystemState state, ref EntityManager entityManager,
            ref EntityCommandBuffer ecb, ref BTInstanceData btInstanceData, ref BlackboardData blackboardData,
            in BTData btData, in Entity owner,
            in Entity btInstance, in BTDelegateData btDelegateData)
        {
            // clean up
            // clean Blackboard
            blackboardData.BoolVars.Dispose();
            blackboardData.IntVars.Dispose();
            blackboardData.FloatVars.Dispose();
            blackboardData.Float2Vars.Dispose();
            blackboardData.Float3Vars.Dispose();
            blackboardData.EntityVars.Dispose();
            blackboardData.QuaternionVars.Dispose();
            blackboardData.StringVars.Dispose();

            // release active AutoReturn nodes
            var autoReturnNodes = entityManager.GetBuffer<BTActiveAutoReturnNodeElement>(btInstance);

            foreach (var element in autoReturnNodes)
            {
                btDelegateData.BTNodeReturnHandlerFunc.Invoke(ref state, ref ecb, ref btInstanceData, ref blackboardData, btData,
                    owner, btInstance, element.NodeId);
            }

            // delete services
            var serviceElements = entityManager.GetBuffer<BTServiceElement>(btInstance);
            foreach (var element in serviceElements)
            {
                ecb.DestroyEntity(element.Service);
            }

            // delete active node
            if (entityManager.HasComponent<BTActiveNodeLink>(btInstance))
                ecb.DestroyEntity(entityManager.GetComponentData<BTActiveNodeLink>(btInstance).ActiveNode);
            ecb.DestroyEntity(btInstance);
        }

        [BurstCompile]
        public static void GetBTree(in DynamicBuffer<BTElement> btElements, in FixedString32Bytes btName,
            out Entity btEntity)
        {
            for (var i = 0; i < btElements.Length; i++)
            {
                if (btElements[i].Name != btName) continue;
                btEntity = btElements[i].BTEntity;
                return;
            }

            btEntity = Entity.Null;
        }
    }
}