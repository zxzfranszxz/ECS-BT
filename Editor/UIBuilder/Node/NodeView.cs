using System;
using SD.ECSBT.BehaviourTree.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.SD.ECSBT.UIBuilder.Node
{
    public sealed class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> OnNodeSelected;

        public readonly NodeDataDto Node;
        public Port Input;
        public Port Output;

        public NodeView(NodeDataDto node) : base("Packages/com.snakydevelop.ecsbt/UIBuilder/BehaviourTree/NodeView.uxml")
        {
            Node = node;
            title = Node.name;
            viewDataKey = Node.guid;

            style.left = Node.position.x;
            style.top = Node.position.y;

            CreateInputPorts();
            CreateOutputPorts();

            SetupClasses();

            // description
            if (Node.Type == null) return;
            var descriptionLabel = this.Q<Label>("description");
            descriptionLabel.text = Node.description;
        }

        private void SetupClasses()
        {
            if (typeof(IActionNode).IsAssignableFrom(Node.Type))
            {
                AddToClassList("action");
            }
            else if (typeof(ICompositeNode).IsAssignableFrom(Node.Type))
            {
                AddToClassList("composite");
            }
            else if (typeof(IDecoratorNode).IsAssignableFrom(Node.Type))
            {
                AddToClassList("decorator");
            }
            else if (typeof(IRootNode).IsAssignableFrom(Node.Type))
            {
                AddToClassList("root");
            }
            else if (typeof(IServiceNode).IsAssignableFrom(Node.Type))
            {
                AddToClassList("service");
            }
        }

        private void CreateInputPorts()
        {
            if (!typeof(IRootNode).IsAssignableFrom(Node.Type))
            {
                Input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
            }

            if (Input == null) return;
            Input.portName = string.Empty;
            Input.style.flexDirection = FlexDirection.Column;
            inputContainer.Add(Input);
        }

        private void CreateOutputPorts()
        {
            if (typeof(ICompositeNode).IsAssignableFrom(Node.Type))
            {
                Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            }
            else if (typeof(IDecoratorNode).IsAssignableFrom(Node.Type) || typeof(IRootNode).IsAssignableFrom(Node.Type) ||
                     typeof(IServiceNode).IsAssignableFrom(Node.Type))
            {
                Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            }

            if (Output == null) return;
            Output.portName = string.Empty;
            Output.style.flexDirection = FlexDirection.ColumnReverse;
            outputContainer.Add(Output);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Node.position = new Vector2(newPos.xMin, newPos.yMin);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            OnNodeSelected?.Invoke(null);
        }
    }
}