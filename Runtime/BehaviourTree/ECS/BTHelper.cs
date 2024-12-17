using System;
using System.Runtime.CompilerServices;
using Game.ECS.AI.BehaviourTree.Blackboard;
using Game.ECS.AI.BehaviourTree.Components;
using Game.ECS.AI.BehaviourTree.Components.Nodes;
using Game.ECS.AI.BehaviourTree.Instance;
using Game.ECS.AI.BehaviourTree.Services;
using SD.ECSBT.BehaviourTree.Nodes;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Game.ECS.AI.BehaviourTree.Core
{
    [BurstCompile]
    public static class BTHelper
    {
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
        public static void CleanupBTInstance(ref EntityManager entityManager, ref EntityCommandBuffer ecb,
            in Entity btInstance)
        {
            // clean up
            // clean Blackboard
            var blackboardData = entityManager.GetComponentData<BlackboardData>(btInstance);
            blackboardData.BoolVars.Dispose();
            blackboardData.IntVars.Dispose();
            blackboardData.FloatVars.Dispose();
            blackboardData.Float2Vars.Dispose();
            blackboardData.Float3Vars.Dispose();
            blackboardData.EntityVars.Dispose();
            blackboardData.QuaternionVars.Dispose();
            blackboardData.StringVars.Dispose();

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