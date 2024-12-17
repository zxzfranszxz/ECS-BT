using System.IO;
using System.Linq;
using UnityEditor;
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

            var processSystemClassName = new TextField("");
            processSystemClassName.value = _settings.processSystemClassName;
            processSystemClassName.RegisterValueChangedCallback(evt => _settings.processSystemClassName = evt.newValue);
            root.Add(processSystemClassName);

            // Create button
            var saveButton = new Button();
            saveButton.text = "Save";
            saveButton.clicked += () =>
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
            };
            root.Add(saveButton);
        }
    }
}