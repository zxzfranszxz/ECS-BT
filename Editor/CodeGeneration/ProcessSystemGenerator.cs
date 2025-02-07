using Editor.SD.ECSBT.Settings;
using UnityEditor;

namespace Editor.SD.ECSBT.CodeGeneration
{
    public static class ProcessSystemGenerator
    {
        public static void Generate(BTSettingsData settings)
        {
            var generatedCode = string.Format(Template, settings.nodeHandlerNamespace, settings.nodeHandlerNamespace,
                settings.processSystemClassName, settings.nodeHandlerClassName);

            System.IO.File.WriteAllText(
                $"{settings.processSystemClassPath}/{settings.processSystemClassName}.generated.cs", generatedCode);
            AssetDatabase.Refresh();
        }

        private const string Template = @"
using {0};
using SD.ECSBT.BehaviourTree.ECS;
using SD.ECSBT.BehaviourTree.ECS.Blackboard;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.ECS.Process;
using SD.ECSBT.BehaviourTree.ECS.Services;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Random = Unity.Mathematics.Random;

namespace {1}
{{
    [UpdateInGroup(typeof(BTLogicSystemGroup))]
    [UpdateAfter(typeof(BTAbortSystem))]
    public partial struct {2} : ISystem
    {{
        public Random random;

        public void OnCreate(ref SystemState state)
        {{
            random = new Random((uint) UnityEngine.Random.Range(uint.MinValue, uint.MaxValue));
        }}

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {{
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;

            foreach (var (btInstanceRW, blackboardDataRW, ownerRO, entity) in SystemAPI
                         .Query<RefRW<BTInstanceData>, RefRW<BlackboardData>, RefRO<BTOwner>>()
                         .WithAll<BTLogicEnabled>().WithEntityAccess())
            {{
                ref var btInstanceData = ref btInstanceRW.ValueRW;
                ref var blackboardData = ref blackboardDataRW.ValueRW;
                if (!SystemAPI.HasComponent<BTData>(btInstanceData.BehaviorTree)) continue;
                ref readonly var btData = ref SystemAPI.GetComponentRO<BTData>(btInstanceData.BehaviorTree).ValueRO;
                var owner = ownerRO.ValueRO.Value;

                if (btInstanceData is {{ RunState: BTRunState.None, ActiveNodeId: 0 }})
                {{
                    btInstanceData.RunState = BTRunState.Digging;
                }}

                while (true)
                {{
                    if (btInstanceData.RunState == BTRunState.Digging)
                    {{
                        {3}.RunNode(ref state, ref this, ref ecb, ref btInstanceData, ref blackboardData, in btData, in owner, in entity, out var result);
                        btInstanceData.ActiveNodeState = result;
                        if (result == ActiveNodeState.Running)
                        {{
                            SystemAPI.SetComponentEnabled<BTLogicEnabled>(entity, false);
                            break;
                        }}
                        BTNodeResultHandler.ReturnNodeResult(ref btInstanceData, in btData);
                    }}
                    else if (btInstanceData.RunState == BTRunState.Returning)
                    {{
                        if (btData.Nodes[btInstanceData.ActiveNodeId].NodeType == NodeType.Service)
                        {{
                            BTServiceHelper.SetActiveService(ref entityManager, btInstanceData.ActiveNodeId, entity,
                                false);
                        }}

                        BTNodeResultHandler.ReturnNodeResult(ref btInstanceData, in btData);
                    }}
                    else
                    {{
                        break;
                    }}
                }}
            }}

            ecb.Playback(entityManager);
            ecb.Dispose();
        }}

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {{
        }}
    }}
}}";
    }
}