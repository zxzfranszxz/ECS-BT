using Editor.SD.ECSBT.UIBuilder.Node;
using SD.ECSBT.BehaviourTree;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using InspectorView = Editor.SD.ECSBT.UIBuilder.Common.InspectorView;

namespace Editor.SD.ECSBT.UIBuilder.BehaviourTree
{
    public class BehaviourTreeEditor : EditorWindow
    {
        private BehaviourTreeView _treeView;
        private InspectorView _inspectorView;
        private IMGUIContainer _blackboardView;

        [SerializeField] private VisualTreeAsset visualTreeAsset;
        [SerializeField] private StyleSheet styleSheet;

        private SerializedObject _treeObject;
        private SerializedProperty _backboardProperty;

        [MenuItem("BehaviourTree/Editor")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject is not BehaviourTreeSO) return false;
            OpenWindow();
            return true;
        }

        public void CreateGUI()
        {
            visualTreeAsset.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(styleSheet);

            _treeView = rootVisualElement.Q<BehaviourTreeView>();
            _inspectorView = rootVisualElement.Q<InspectorView>();
            _blackboardView = rootVisualElement.Q<IMGUIContainer>();
            _blackboardView.onGUIHandler = () =>
            {
                if (_treeObject == null) return;
                _treeObject.Update();
                EditorGUILayout.PropertyField(_backboardProperty);
                _treeObject.ApplyModifiedProperties();
            };
            _treeView.OnNodeSelected = OnNodeSelectionChanged;
            OnSelectionChange();
        }

        private void OnNodeSelectionChanged(NodeView node)
        {
            _inspectorView.UpdateSelection(node, _treeObject.targetObject as BehaviourTreeSO);
        }

        private void OnSelectionChange()
        {
            var tree = Selection.activeObject as BehaviourTreeSO;
            if (tree && AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
            {
                _treeView.PopulateView(tree);
            }

            if (tree)
            {
                _treeObject = new SerializedObject(tree);
                _backboardProperty = _treeObject.FindProperty("blackboard");
            }
        }
    }
}