using System;
using System.Globalization;
using System.Linq;
using SD.ECSBT.BehaviourTree.Data;
using UnityEditor;
using UnityEngine;
using Utils.Vector;

namespace Editor.SD.ECSBT.Property
{
    [CustomPropertyDrawer(typeof(BTVar))]
    public class BTVarDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeReference = property.FindPropertyRelative("typeReference");
            var type = Type.GetType(typeReference.stringValue);
            if (type == typeof(Vector2) || type == typeof(Vector3))
            {
                return EditorGUIUtility.singleLineHeight * 4 + 10;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight * 3 + 10;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Find the properties
            var typeReference = property.FindPropertyRelative("typeReference");
            var id = property.FindPropertyRelative("id");
            var value = property.FindPropertyRelative("value");

            // Calculate rects
            var typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var idRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width,
                EditorGUIUtility.singleLineHeight);
            var valueRect = new Rect(position.x, position.y + 2 * (EditorGUIUtility.singleLineHeight + 2),
                position.width, EditorGUIUtility.singleLineHeight);

            // Draw fields - varType and id
            if (string.IsNullOrEmpty(typeReference.stringValue))
                typeReference.stringValue = typeof(string).AssemblyQualifiedName;
            var currentType = Type.GetType(typeReference.stringValue) ?? typeof(string);

            var supportedTypes = BTVar.SupportedTypes().ToList();
            var contents = BTVar.SupportedTypes().Select(type => new GUIContent(type.Name
            )).ToArray();
            var selectedItem = EditorGUI.Popup(typeRect, supportedTypes.IndexOf(currentType), contents, EditorStyles.popup);
            typeReference.stringValue = supportedTypes[selectedItem].AssemblyQualifiedName;

            EditorGUI.PropertyField(idRect, id, new GUIContent("Id:"));

            // Draw the appropriate field based on varType
            currentType = Type.GetType(typeReference.stringValue);
            if (currentType == typeof(bool))
            {
                bool.TryParse(value.stringValue, out var boolValue);
                boolValue = EditorGUI.Toggle(valueRect, "Value:", boolValue);
                value.stringValue = boolValue.ToString();
            }
            else if (currentType == typeof(int))
            {
                int.TryParse(value.stringValue, out var intValue);
                intValue = EditorGUI.IntField(valueRect, "Value:", intValue);
                value.stringValue = intValue.ToString();
            }
            else if (currentType == typeof(float))
            {
                float.TryParse(value.stringValue, out var floatValue);
                floatValue = EditorGUI.FloatField(valueRect, "Value:", floatValue);
                value.stringValue = floatValue.ToString(CultureInfo.InvariantCulture);
            }
            else if (currentType == typeof(Vector2))
            {
                VectorHelper.TryParseVector2(value.stringValue, out var vector2Value);
                vector2Value = EditorGUI.Vector2Field(valueRect, "Value:", vector2Value);
                value.stringValue = vector2Value.ToString("F2");
            }
            else if (currentType == typeof(Vector3))
            {
                VectorHelper.TryParseVector3(value.stringValue, out var vectorValue);
                vectorValue = EditorGUI.Vector3Field(valueRect, "Value:", vectorValue);
                value.stringValue = vectorValue.ToString("F2");
            }
            else if (currentType == typeof(string))
            {
                value.stringValue = EditorGUI.TextField(valueRect, "Value:", value.stringValue);
            }
            else if (currentType.IsEnum)
            {
                var abortType = (Enum)Enum.Parse(currentType, value.stringValue);
                value.stringValue = EditorGUI.EnumPopup(valueRect, abortType).ToString();
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            EditorGUI.EndProperty();
        }
    }
}