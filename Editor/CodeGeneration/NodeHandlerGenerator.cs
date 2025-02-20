using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Editor.SD.ECSBT.Settings;
using SD.ECSBT.BehaviourTree.ECS.Nodes;
using SD.ECSBT.BehaviourTree.Nodes.Attributes;
using UnityEditor;

namespace Editor.SD.ECSBT.CodeGeneration
{
    [InitializeOnLoad]
    public static class NodeHandlerGenerator
    {
        static NodeHandlerGenerator()
        {
            var settingsDataGuid = AssetDatabase.FindAssets("t:BTSettingsData", new[] { "Assets" }).FirstOrDefault();

            if (settingsDataGuid == null)
            {
                return;
            }

            var settingsPath = AssetDatabase.GUIDToAssetPath(settingsDataGuid);
            var settings = AssetDatabase.LoadAssetAtPath<BTSettingsData>(settingsPath);

            var nodeHandleMethods = TypeCache.GetMethodsWithAttribute<NodeHandlerAttribute>().OrderBy(m => m.Name.GetHashCode()).ToList();

            var namespaces = new HashSet<string>();
            foreach (var nodeHandleMethod in nodeHandleMethods)
            {
                if (nodeHandleMethod.DeclaringType == null) continue;
                namespaces.Add(nodeHandleMethod.DeclaringType.Namespace);
            }

            // generate
            var sb = new StringBuilder();
            // add namespaces
            foreach (var nse in namespaces)
            {
                sb.AppendLine($"using {nse};");
            }
            
            sb.AppendFormat(StartPart, settings.nodeHandlerNamespace, settings.nodeHandlerClassName);

            var packageAssembly = typeof(AIRootNode).Assembly;
            
            // switch part
            foreach (var nodeHandleMethod in nodeHandleMethods)
            {
                if(nodeHandleMethod.Name.Contains("$BurstManaged")) continue;
                var isPackageHandler = nodeHandleMethod.DeclaringType!.Assembly == packageAssembly;
                var typeHash = nodeHandleMethod.GetCustomAttribute<NodeHandlerAttribute>().StableTypeHash;

                var systemParam = isPackageHandler ? string.Empty : "ref system,";
                sb.Append($@"
                case {typeHash}:
                    {nodeHandleMethod.DeclaringType!.Name}.{nodeHandleMethod.Name}(
                        ref systemState, {systemParam} ref ecb, ref btInstanceData, ref blackboardData,
                        in btData, in owner, in btInstance, in node, out activeNodeState);
                    break;");
            }
            
            sb.Append(EndPart);
            
            System.IO.File.WriteAllText(
                $"{settings.nodeHandlerClassPath}/{settings.nodeHandlerClassName}.generated.cs", sb.ToString());
            AssetDatabase.Refresh();
        }


        private const string StartPart = 
@"using SD.ECSBT.BehaviourTree.ECS.Blackboard;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Instance;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.ECS.Services;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace {0}
{{
    [BurstCompile]
    public static class {1}
    {{
        [BurstCompile]
        public static void RunNode(ref SystemState systemState, ref BTProcessSystem system, ref EntityCommandBuffer ecb,
            ref BTInstanceData btInstanceData, ref BlackboardData blackboardData, in BTData btData, in Entity owner, 
            in Entity btInstance, out ActiveNodeState activeNodeState)
        {{
            var node = btData.Nodes[btInstanceData.ActiveNodeId];
            var nodeType = node.NodeComponentType;

            activeNodeState = ActiveNodeState.None;

            if(node.NodeType is NodeType.Root or NodeType.Composite)
            {{
                activeNodeState = ActiveNodeState.Success;
                return;
            }}

            var entityManager = systemState.EntityManager;
            if(node.NodeType is NodeType.Service)
            {{
                BTServiceHelper.SetActiveService(ref entityManager, in node.Id, in btInstance, true);
            }}

            switch (node.StableTypeHash)
            {{";

        private const string EndPart = @"
            }

            if (activeNodeState != ActiveNodeState.None) return;
            Debug.LogError($""Node {nodeType.ToFixedString()} is not implemented."");
            activeNodeState = ActiveNodeState.Success;
        }
    }
}";
    }
}