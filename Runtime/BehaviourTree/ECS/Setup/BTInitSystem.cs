using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SD.ECSBT.BehaviourTree.Data;
using SD.ECSBT.BehaviourTree.ECS.Components;
using SD.ECSBT.BehaviourTree.ECS.Nodes.Data;
using SD.ECSBT.BehaviourTree.Nodes;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Utils.Vector;

namespace SD.ECSBT.BehaviourTree.ECS.Setup
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct BTInitSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BTDataSOElement>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var buffer = SystemAPI.GetSingletonBuffer<BTDataSOElement>();
            var bufferEntity = SystemAPI.GetSingletonEntity<BTDataSOElement>();

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var btBufferEntity = state.EntityManager.CreateSingletonBuffer<BTElement>();

            foreach (var btDataSOElement in buffer)
            {
                var btDataSO = btDataSOElement.BTData.Value;

                var btEntity = ecb.CreateEntity();
                ecb.SetName(btEntity, btDataSO.name);
                
                var btData = new BTData
                {
                    BTName = btDataSO.name,
                    Nodes = new NativeArray<NodeData>(btDataSO.nodes.Count, Allocator.Persistent)
                };
                
                var rootNode = btDataSO.RootNode;
                var nodeId = 0; 
                
                // add all nodes recursively
                AddNodes(ref btData, ref nodeId, rootNode, btDataSO.nodes);
                
                ecb.AddComponent(btEntity, btData);
                ecb.AppendToBuffer(btBufferEntity, new BTElement { Name = btDataSO.name, BTEntity = btEntity });
            }

            state.EntityManager.DestroyEntity(bufferEntity);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static void AddNodes(ref BTData btData, ref int nodeId, NodeDataDto nodeDataDto, List<NodeDataDto> nodesDto)
        {
            var nodeData = new NodeData
            {
                Id = nodeId,
                StableTypeHash = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(nodeDataDto.Type)).StableTypeHash,
                NodeComponentType = ComponentType.ReadWrite(nodeDataDto.Type),
                NodeType = BTHelper.GetNodeType(nodeDataDto.Type),
                Guid = nodeDataDto.Guid
            };
            
            // 1) add node vars
            if (nodeDataDto.vars.Count > 0 || nodeDataDto.blackboardVars.Count > 0)
            {
                InitNodeVars(ref nodeData, ref nodeDataDto);
            }
            
            // 2) setup children
            
            // children sorted by priority position from left to right
            var children = nodesDto.Where(dto => dto.ParentGuid == nodeData.Guid).OrderBy(dto => dto.position.x).ToList();
            
            btData.Nodes[nodeData.Id] = nodeData;
            nodeId++;
            foreach (var dataDto in children)
            {
                AddNodes(ref btData, ref nodeId, dataDto, nodesDto);
            }
            
            // 2.1) set children
            if (children.Count > 0)
            {
                nodeData.Children = new NativeArray<int>(children.Count, Allocator.Persistent);
                var i = 0;
                foreach (var node in btData.Nodes)
                {
                    if (children.All(dto => dto.Guid != node.Guid)) continue;
                    nodeData.Children[i] = node.Id;
                    i++;
                }
            }
            
            // 2.2) set parent
            foreach (var node in btData.Nodes)
            {
                if (nodeDataDto.ParentGuid != node.Guid) continue;
                nodeData.ParentId = node.Id;
                break;
            }
            
            btData.Nodes[nodeData.Id] = nodeData;
        }

        private static void InitNodeVars(ref NodeData nodeData, ref NodeDataDto nodeDataDto)
        {
            // bool
            var vars = nodeDataDto.vars.Where(var => var.Type == typeof(bool)).ToList();
            if(vars.Any())
            {
                nodeData.BoolVars = new NativeHashMap<FixedString32Bytes, bool>(vars.Count, Allocator.Persistent);
                foreach (var btVar in vars)
                {
                    nodeData.BoolVars.Add(btVar.id, bool.Parse(btVar.value));
                }
            }
            
            // int
            vars = nodeDataDto.vars.Where(var => var.Type == typeof(int)).ToList();
            if(vars.Any())
            {
                nodeData.IntVars = new NativeHashMap<FixedString32Bytes, int>(vars.Count, Allocator.Persistent);
                foreach (var btVar in vars)
                {
                    nodeData.IntVars.Add(btVar.id, int.Parse(btVar.value));
                }
            }
            
            // float
            vars = nodeDataDto.vars.Where(var => var.Type == typeof(float)).ToList();
            if(vars.Any())
            {
                nodeData.FloatVars = new NativeHashMap<FixedString32Bytes, float>(vars.Count, Allocator.Persistent);
                foreach (var btVar in vars)
                {
                    nodeData.FloatVars.Add(btVar.id, float.Parse(btVar.value, NumberStyles.Float, CultureInfo.InvariantCulture));
                }
            }
            
            // string
            vars = nodeDataDto.vars.Where(var => var.Type == typeof(string)).ToList();
            if(vars.Any() || nodeDataDto.blackboardVars.Any())
            {
                var capacity = vars.Count + nodeDataDto.blackboardVars.Count;
                nodeData.StringVars = new NativeHashMap<FixedString32Bytes, FixedString32Bytes>(capacity, Allocator.Persistent);
                foreach (var btVar in vars)
                {
                    nodeData.StringVars.Add(btVar.id, btVar.value);
                }
                foreach (var varLink in nodeDataDto.blackboardVars)
                {
                    nodeData.StringVars.Add(varLink.id, varLink.varId);
                }
            }
            
            // float2
            vars = nodeDataDto.vars.Where(var => var.Type == typeof(Vector2)).ToList();
            if(vars.Any())
            {
                nodeData.Float2Vars = new NativeHashMap<FixedString32Bytes, float2>(vars.Count, Allocator.Persistent);
                foreach (var btVar in vars)
                {
                    VectorHelper.TryParseVector2(btVar.value, out var value);
                    nodeData.Float2Vars.Add(btVar.id, value);
                }
            }
            
            // float3
            vars = nodeDataDto.vars.Where(var => var.Type == typeof(Vector3)).ToList();
            if(vars.Any())
            {
                nodeData.Float3Vars = new NativeHashMap<FixedString32Bytes, float3>(vars.Count, Allocator.Persistent);
                foreach (var btVar in vars)
                {
                    VectorHelper.TryParseVector3(btVar.value, out var value);
                    nodeData.Float3Vars.Add(btVar.id, value);
                }
            }
            
            // AbortType
            vars = nodeDataDto.vars.Where(var => var.Type == typeof(AbortType)).ToList();
            if(vars.Any())
            {
                if(!nodeData.IntVars.IsCreated)
                    nodeData.IntVars = new NativeHashMap<FixedString32Bytes, int>(vars.Count, Allocator.Persistent);
                foreach (var btVar in vars)
                {
                    nodeData.IntVars.Add(btVar.id, (int)Enum.Parse(btVar.Type, btVar.value));
                }
            }
            
            // NotifyType
            vars = nodeDataDto.vars.Where(var => var.Type == typeof(NotifyType)).ToList();
            if(vars.Any())
            {
                if(!nodeData.IntVars.IsCreated)
                    nodeData.IntVars = new NativeHashMap<FixedString32Bytes, int>(vars.Count, Allocator.Persistent);
                foreach (var btVar in vars)
                {
                    nodeData.IntVars.Add(btVar.id, (int)Enum.Parse(btVar.Type, btVar.value));
                }
            }
            
            // BlackboardConditionType
            vars = nodeDataDto.vars.Where(var => var.Type == typeof(BlackboardConditionType)).ToList();
            if(vars.Any())
            {
                if(!nodeData.IntVars.IsCreated)
                    nodeData.IntVars = new NativeHashMap<FixedString32Bytes, int>(vars.Count, Allocator.Persistent);
                foreach (var btVar in vars)
                {
                    nodeData.IntVars.Add(btVar.id, (int)Enum.Parse(btVar.Type, btVar.value));
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            foreach (var btData in SystemAPI.Query<RefRW<BTData>>())
            {
                if (btData.ValueRW.Nodes.IsCreated)
                {
                    for (var i = 0; i < btData.ValueRW.Nodes.Length; i++)
                    {
                        var nodeData = btData.ValueRW.Nodes[i];
                        nodeData.Children.Dispose();

                        nodeData.BoolVars.Dispose();
                        nodeData.IntVars.Dispose();
                        nodeData.FloatVars.Dispose();
                        nodeData.Float2Vars.Dispose();
                        nodeData.Float3Vars.Dispose();
                        nodeData.StringVars.Dispose();
                    }

                    btData.ValueRW.Nodes.Dispose();
                }
            }
        }
    }
}