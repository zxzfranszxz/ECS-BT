using System;
using System.Linq;
using SD.ECSBT.BehaviourTree.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.SD.ECSBT.Property
{
    [CustomPropertyDrawer(typeof(BTVarInfo))]
    public class BTVarInfoDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 10;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var id = property.FindPropertyRelative("id");
            var typeReference = property.FindPropertyRelative("typeReference");
            
            var idRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var typeRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width,
                EditorGUIUtility.singleLineHeight);
            
            EditorGUI.PropertyField(idRect, id, new GUIContent("Id:"));
            
            if (string.IsNullOrEmpty(typeReference.stringValue))
                typeReference.stringValue = typeof(string).AssemblyQualifiedName;
            var currentType = Type.GetType(typeReference.stringValue!);
            
            var supportedTypes = BTVar.SupportedTypes().ToList();
            var contents = BTVar.SupportedTypes().Select(type => new GUIContent(type.Name
            )).ToArray();
            var selectedItem = EditorGUI.Popup(typeRect, supportedTypes.IndexOf(currentType), contents, EditorStyles.popup);
            typeReference.stringValue = supportedTypes[selectedItem].AssemblyQualifiedName;
            EditorGUI.EndProperty();
        }
    }
}