using System.IO;
using System.Linq;
using Editor.SD.ECSBT.CodeGeneration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.SD.ECSBT.Settings
{
    public class BTSettingsWindow : EditorWindow
    {
        private BTSettingsData _settings;

        [MenuItem("BehaviourTree/Settings")]
        public static void ShowExample()
        {
            var wnd = GetWindow<BTSettingsWindow>();
            wnd.titleContent = new GUIContent("BT Settings");
        }

        private void Awake()
        {
            var settingsDataGuid = AssetDatabase.FindAssets("t:BTSettingsData", new[] { "Assets" }).FirstOrDefault();
            
            if (settingsDataGuid == null)
            {
                const string path = "Assets/ECSBT";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                _settings = CreateInstance<BTSettingsData>();
                AssetDatabase.CreateAsset(_settings, AssetDatabase.GenerateUniqueAssetPath($"{path}/BTSettingsData.asset"));
            }
            else
            {
                _settings = AssetDatabase.LoadAssetAtPath<BTSettingsData>(AssetDatabase.GUIDToAssetPath(settingsDataGuid));
            }
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;

            var serializedObject = new SerializedObject(_settings);
            var inspectorElement = new InspectorElement(serializedObject);
            rootVisualElement.Add(inspectorElement);

            // Create button
            var saveButton = new Button();
            saveButton.text = "Save";
            saveButton.clicked += () =>
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
            };
            root.Add(saveButton);
            
            var generateButton = new Button();
            generateButton.text = "Generate";
            generateButton.clicked += () =>
            {
                ProcessSystemGenerator.Generate(_settings);
            };
            root.Add(generateButton);
        }
    }
}