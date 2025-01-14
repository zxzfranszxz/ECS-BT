using System;
using System.Collections.Generic;
using System.Linq;
using SD.ECSBT.BehaviourTree.Nodes;
using SD.ECSBT.BehaviourTree.Nodes.Attributes;
using UnityEditor;
using UnityEngine;

namespace SD.ECSBT.BehaviourTree
{
    [CreateAssetMenu(fileName = "BehaviourTree", menuName = "AI/BehaviourTree", order = 0)]
    public class BehaviourTreeSO : ScriptableObject
    {
        public List<NodeDataDto> nodes;
        public BlackboardDataForSo blackboard;
        
        public NodeDataDto RootNode => nodes.FirstOrDefault(dto => typeof(IRootNode).IsAssignableFrom(dto.Type));

        public NodeDataDto CreateNode(Type nodeType)
        {
            var nodeName = nodeType.Name;
            if (Attribute.IsDefined(nodeType, typeof(NameAttribute)))
            {
                nodeName = (Attribute.GetCustomAttribute(nodeType, typeof(NameAttribute)) as NameAttribute)?.Name;
            }
            var node = new NodeDataDto
            {
                typeReference = nodeType.AssemblyQualifiedName,
                guid = Guid.NewGuid().ToString(),
                name = nodeName,
            };
            
            nodes.Add(node);
            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(NodeDataDto node)
        {
            nodes.Remove(node);
            AssetDatabase.SaveAssets();
        }

        public void AddChild(NodeDataDto parent, NodeDataDto child)
        {
            
            if (typeof(IDecoratorNode).IsAssignableFrom(parent.Type))
            {
                // reset parent
                parent.childrenGuids.ForEach(_ => nodes.First(node => node.parentGuid == parent.guid).parentGuid = string.Empty);
                // make list empty
                parent.childrenGuids.Clear();
            }
            
            // add
            parent.childrenGuids.Add(child.guid);
            child.parentGuid = parent.guid;
        }

        public void RemoveChild(NodeDataDto parent, NodeDataDto child)
        {
            nodes.First(node => node.guid == child.guid).parentGuid = string.Empty;
            parent.childrenGuids.Remove(child.guid);
        }
    }
}