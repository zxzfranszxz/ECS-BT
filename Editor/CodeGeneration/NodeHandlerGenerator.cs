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
            var nodeReturnHandleMethods = TypeCache.GetMethodsWithAttribute<NodeReturnHandlerAttribute>().OrderBy(m => m.Name.GetHashCode()).ToList();

            var namespaces = new HashSet<string>();
            foreach (var nodeHandleMethod in nodeHandleMethods)
            {
                if (nodeHandleMethod.DeclaringType == null) continue;
                namespaces.Add(nodeHandleMethod.DeclaringType.Namespace);
            }
            foreach (var nodeHandleMethod in nodeReturnHandleMethods)
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
            
            sb.AppendFormat(START_PART, settings.nodeHandlerNamespace, settings.nodeHandlerClassName);

            var packageAssembly = typeof(AIRootNode).Assembly;
            
            // run switch part
            foreach (var nodeHandleMethod in nodeHandleMethods)
            {
                if(nodeHandleMethod.Name.Contains("$BurstManaged")) continue;
                var isPackageHandler = nodeHandleMethod.DeclaringType!.Assembly == packageAssembly;
                var typeHash = nodeHandleMethod.GetCustomAttribute<NodeHandlerAttribute>().StableTypeHash;

                var systemParam = isPackageHandler ? string.Empty : "ref system,";
                sb.Append($@"
                case {typeHash}:
                    {nodeHandleMethod.DeclaringType!.Name}.{nodeHandleMethod.Name}(ref systemState, {systemParam} ref ecb, in btInstance, in btData, in node, out activeNodeState);
                    break;");
            }
            
            sb.Append(MID_PART);
            
            // return switch part
            foreach (var nodeHandleMethod in nodeReturnHandleMethods)
            {
                if(nodeHandleMethod.Name.Contains("$BurstManaged")) continue;
                var typeHash = nodeHandleMethod.GetCustomAttribute<NodeReturnHandlerAttribute>().StableTypeHash;
                
                sb.Append($@"
                case {typeHash}:
                    {nodeHandleMethod.DeclaringType!.Name}.{nodeHandleMethod.Name}(ref systemState, ref ecb, in btInstance, in btData, in node);
                    break;");
            }
            
            sb.Append(END_PART);
            
            System.IO.File.WriteAllText(
                $"{settings.nodeHandlerClassPath}/{settings.nodeHandlerClassName}.generated.cs", sb.ToString());
            AssetDatabase.Refresh();
        }


        private const string START_PART = 
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
            in BTInstanceAspect btInstance, in BTData btData, out ActiveNodeState activeNodeState)
        {{
            var node = btData.Nodes[btInstance.InstanceDataRW.ValueRO.ActiveNodeId];
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
                BTServiceHelper.SetActiveService(ref entityManager, in node.Id, in btInstance.Entity, true);
            }}

            switch (node.StableTypeHash)
            {{";
        
        private const string MID_PART = @"
            }

            if (activeNodeState != ActiveNodeState.None) return;
            Debug.LogError($""Node {nodeType.ToFixedString()} is not implemented."");
            activeNodeState = ActiveNodeState.Success;
        }

        [BurstCompile]
        public static void ReturnNode(ref SystemState systemState, ref EntityCommandBuffer ecb,
            in BTInstanceAspect btInstance, in BTData btData, in int nodeId = -1)
        {
            var id = nodeId >= 0 ? nodeId : btInstance.InstanceDataRW.ValueRO.ActiveNodeId;
            var node = btData.Nodes[id];
            var nodeType = node.NodeComponentType;

            var entityManager = systemState.EntityManager;
            if(node.NodeType is NodeType.Service)
            {
                BTServiceHelper.SetActiveService(ref entityManager, in node.Id, in btInstance.Entity, false);
                return;
            }

            if(node.NodeType is NodeType.Action)
            {
                return;
            }

            switch (node.StableTypeHash)
            {";

        private const string END_PART = @"
                
            }
        }
    }
}";
    }
}