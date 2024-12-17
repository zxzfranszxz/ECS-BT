using System;
using System.Collections.Generic;
using System.Linq;
using Editor.SD.ECSBT.UIBuilder.Node;
using Game.ECS.AI.BehaviourTree.Nodes;
using SD.ECSBT.BehaviourTree;
using SD.ECSBT.BehaviourTree.Nodes;
using SD.ECSBT.BehaviourTree.Nodes.Attributes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.SD.ECSBT.UIBuilder.BehaviourTree
{
    [UxmlElement]
    public partial class BehaviourTreeView : GraphView
    {
        private BehaviourTreeSO _tree;
        
        public Action<NodeView> OnNodeSelected;

        public BehaviourTreeView()
        {
            Insert(0, new GridBackground { name = "GridBackground" });

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.snakydevelop.ecsbt/UIBuilder/BehaviourTree/BehaviourTreeEditor.uss");
            styleSheets.Add(styleSheet);
        }

        public void PopulateView(BehaviourTreeSO tree)
        {
            _tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            // root node
            if (_tree.RootNode == null || _tree.RootNode.Type != typeof(AIRootNode))
            { 
                _tree.CreateNode(typeof(AIRootNode));
                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }
            
            // create node view
            _tree.nodes.ForEach(CreateNodeView);
            
            // create edges
            _tree.nodes.ForEach(parentNode =>
            {
                parentNode.childrenGuids.ForEach(childGuid =>
                {
                    var parentView = GetNodeByGuid(parentNode.guid) as NodeView;
                    var childView = GetNodeByGuid(childGuid) as NodeView;

                    if(parentView == null || childView == null) return;
                    var edge = parentView.Output.ConnectTo(childView.Input);
                    AddElement(edge);
                });
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort => endPort.direction != startPort.direction &&
                                                   endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            graphViewChange.elementsToRemove?.ForEach(element =>
            {
                switch (element)
                {
                    case NodeView nodeView:
                        _tree.DeleteNode(nodeView.Node);
                        break;
                    case Edge edge:
                    {
                        if (edge.output.node is not NodeView parentNodeView ||
                            edge.input.node is not NodeView childNodeView) return;
                        _tree.RemoveChild(parentNodeView.Node, childNodeView.Node);
                        break;
                    }
                }
            });

            graphViewChange.edgesToCreate?.ForEach(edge =>
            {
                if (edge.output.node is not NodeView parentNodeView ||
                    edge.input.node is not NodeView childNodeView) return;
                _tree.AddChild(parentNodeView.Node, childNodeView.Node);
            });

            EditorUtility.SetDirty(_tree);
            AssetDatabase.SaveAssets();
            
            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            AddTypes("Action", TypeCache.GetTypesDerivedFrom<IActionNode>());
            AddTypes("Composite", TypeCache.GetTypesDerivedFrom<ICompositeNode>());
            AddTypes("Decorator", TypeCache.GetTypesDerivedFrom<IDecoratorNode>());
            AddTypes("Service", TypeCache.GetTypesDerivedFrom<IServiceNode>());
            
            return;

            void AddTypes(string category, TypeCache.TypeCollection types)
            {
                foreach (var type in types)
                {
                    var nodeName = type.Name;
                    if (Attribute.IsDefined(type, typeof(NameAttribute)))
                    {
                        nodeName = (Attribute.GetCustomAttribute(type, typeof(NameAttribute)) as NameAttribute)?.Name;
                    }

                    if (type.BaseType == null) continue;
                    evt.menu.AppendAction($"{category}/{nodeName}", (a) => AddNode(type, a.eventInfo.mousePosition));
                }
            }
        }

        private void AddNode(Type type, Vector2 mousePosition)
        {
            var node = _tree.CreateNode(type);
            mousePosition = contentViewContainer.WorldToLocal(mousePosition);
            node.position = mousePosition;
            
            CreateNodeView(node);
            EditorUtility.SetDirty(_tree);
            AssetDatabase.SaveAssets();
        }

        private void CreateNodeView(NodeDataDto node)
        {
            var nodeView = new NodeView(node)
            {
                OnNodeSelected = OnNodeSelected
            };
            AddElement(nodeView);
        }
    }
}