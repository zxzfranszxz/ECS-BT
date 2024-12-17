using System.Linq;
using SD.ECSBT.BehaviourTree.Nodes;
using SD.ECSBT.BehaviourTree.Nodes.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor.SD.ECSBT.Property
{
    [CustomPropertyDrawer(typeof(NodeTypePropertyAttribute))]
    public class NodeTypeDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var nodeTypes = TypeCache.GetTypesDerivedFrom<INode>().Where(type => type.IsValueType).ToList();
            var typeNames = nodeTypes.Select(type => type.Name).ToArray();
            var selected = 0;
            if (nodeTypes.Any(type => type.AssemblyQualifiedName == property.stringValue))
            {
                var selectedType = nodeTypes.First(type => type.AssemblyQualifiedName == property.stringValue);
                selected = nodeTypes.IndexOf(selectedType);
            }
            selected = EditorGUI.Popup(position, selected, typeNames, EditorStyles.popup);
            property.stringValue = nodeTypes[selected].AssemblyQualifiedName;

            EditorGUI.EndProperty();
        }
    }
}