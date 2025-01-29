using System;
using System.Globalization;
using System.Linq;
using Editor.SD.ECSBT.UIBuilder.Node;
using SD.ECSBT.BehaviourTree;
using SD.ECSBT.BehaviourTree.Data;
using SD.ECSBT.BehaviourTree.Nodes;
using SD.ECSBT.BehaviourTree.Nodes.Attributes;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Vector;

namespace Editor.SD.ECSBT.UIBuilder.Common
{
    [UxmlElement]
    public partial class InspectorView : VisualElement
    {
        private NodeDataDto _nodeDataDto;
        private BehaviourTreeSO _tree;

        public void UpdateSelection(NodeView nodeView, BehaviourTreeSO tree)
        {
            Clear();

            if(nodeView == null) return;
            
            _tree = tree;
            _nodeDataDto = nodeView.Node;
            InitializeUI();
        }

        private void InitializeUI()
        {
            var nameField = new TextField {
                style =
                {
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold
                },
                value = _nodeDataDto.name
            };
            nameField.RegisterValueChangedCallback(evt => _nodeDataDto.name = evt.newValue);
            Add(nameField);
            
            var descField = new TextField
            {
                style =
                {
                    fontSize = 12
                },
                value = _nodeDataDto.description
            };
            descField.RegisterValueChangedCallback(evt => _nodeDataDto.description = evt.newValue);
            Add(descField);


            AddSeparator();

            // blackboard links

            #region BBLinks

            var blackBoardAttributes = Attribute.GetCustomAttributes(_nodeDataDto.Type, typeof(BlackboardAttribute))
                .Cast<BlackboardAttribute>();

            foreach (var attribute in blackBoardAttributes)
            {
                var horizontalContainer = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                    }
                };
                horizontalContainer.Add(new Label($"{attribute.Name}:") { style = { fontSize = 14 } });
                var dropDown = new DropdownField { style = { flexGrow = 1, } };
                var vars = attribute.Type == typeof(object)
                    ? _tree.blackboard.vars
                    : _tree.blackboard.vars.Where(btVar => btVar.typeReference == attribute.Type.AssemblyQualifiedName);
                foreach (var btVar in vars)
                {
                    dropDown.choices.Add(btVar.id);
                }

                dropDown.RegisterValueChangedCallback(evt =>
                {
                    var varLink = _nodeDataDto.blackboardVars.FirstOrDefault(link => link.id == attribute.Name);
                    if (varLink != null)
                    {
                        varLink.varId = evt.newValue;
                    }
                    else
                    {
                        _nodeDataDto.blackboardVars.Add(new BlackboardVarLink
                            { id = attribute.Name, varId = evt.newValue });
                    }
                });

                //set value
                var varLink = _nodeDataDto.blackboardVars.FirstOrDefault(link => link.id == attribute.Name);
                //check if value exist in blackboard
                if (varLink != null)
                {
                    var bbVar = _tree.blackboard.vars.FirstOrDefault(var => var.id == varLink.varId);
                    if (bbVar == null)
                        varLink.varId = string.Empty;
                }

                dropDown.value = varLink?.varId;

                horizontalContainer.Add(dropDown);
                Add(horizontalContainer);
            }

            #endregion

            AddSeparator();

            // node vars

            #region NodeVars

            var nodeVarAttributes = Attribute.GetCustomAttributes(_nodeDataDto.Type, typeof(NodeVarAttribute))
                .Cast<NodeVarAttribute>();

            foreach (var attribute in nodeVarAttributes)
            {
                CreateIfNotExists(attribute.Id, attribute.Type);
                var horizontalContainer = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                    }
                };
                horizontalContainer.Add(new Label($"{attribute.Id}:") { style = { fontSize = 14 } });
                if (attribute.Type == typeof(bool))
                {
                    var boolField = new Toggle
                    {
                        style = { flexGrow = 1 },
                        value = bool.Parse(GetNodeVarValue(attribute.Id))
                    };
                    boolField.RegisterValueChangedCallback(evt =>
                        SetNodeVarValue(attribute.Id, evt.newValue.ToString(), attribute.Type));
                    horizontalContainer.Add(boolField);
                }
                else if (attribute.Type == typeof(int))
                {
                    var intField = new IntegerField
                    {
                        style = { flexGrow = 1 },
                        value = int.Parse(GetNodeVarValue(attribute.Id))
                    };
                    intField.RegisterValueChangedCallback(evt =>
                        SetNodeVarValue(attribute.Id, evt.newValue.ToString(), attribute.Type));
                    horizontalContainer.Add(intField);
                }
                else if (attribute.Type == typeof(float))
                {
                    var floatField = new FloatField
                    {
                        style = { flexGrow = 1 },
                        value = float.Parse(GetNodeVarValue(attribute.Id), NumberStyles.Float, CultureInfo.InvariantCulture)
                    };
                    floatField.RegisterValueChangedCallback(evt =>
                        SetNodeVarValue(attribute.Id, evt.newValue.ToString(CultureInfo.InvariantCulture),
                            attribute.Type));
                    horizontalContainer.Add(floatField);
                }
                else if (attribute.Type == typeof(string))
                {
                    var textField = new TextField
                    {
                        style = { flexGrow = 1 },
                        value = GetNodeVarValue(attribute.Id)
                    };
                    textField.RegisterValueChangedCallback(evt =>
                        SetNodeVarValue(attribute.Id, evt.newValue, attribute.Type));
                    horizontalContainer.Add(textField);
                }
                else if (attribute.Type == typeof(Vector2))
                {
                    var vector2Field = new Vector2Field { style = { flexGrow = 1 } };
                    VectorHelper.TryParseVector2(GetNodeVarValue(attribute.Id), out var vector2Value);
                    vector2Field.value = vector2Value;
                    vector2Field.RegisterValueChangedCallback(evt =>
                        SetNodeVarValue(attribute.Id, evt.newValue.ToString(), attribute.Type));
                    horizontalContainer.Add(vector2Field);
                }
                else if (attribute.Type == typeof(Vector3))
                {
                    var vector3Field = new Vector3Field { style = { flexGrow = 1 } };
                    VectorHelper.TryParseVector3(GetNodeVarValue(attribute.Id), out var vector3Value);
                    vector3Field.value = vector3Value;
                    vector3Field.RegisterValueChangedCallback(evt =>
                        SetNodeVarValue(attribute.Id, evt.newValue.ToString(), attribute.Type));
                    horizontalContainer.Add(vector3Field);
                }
                else if (attribute.Type.IsEnum)
                {
                    var options = Enum.GetNames(attribute.Type).ToList();
                    var abortType = Enum.Parse(attribute.Type, GetNodeVarValue(attribute.Id));
                    var dropdown = new DropdownField(options, (int)abortType);
                    dropdown.RegisterValueChangedCallback(evt =>
                    {
                        SetNodeVarValue(attribute.Id, evt.newValue, attribute.Type);
                    });
                    horizontalContainer.Add(dropdown);
                }

                Add(horizontalContainer);
            }

            #endregion

            return;

            void CreateIfNotExists(string id, Type type)
            {
                if (_nodeDataDto.vars.FirstOrDefault(var => var.id == id) != null) return;
                var defaultValue = string.Empty;
                if (type == typeof(bool))
                {
                    defaultValue = false.ToString();
                }
                else if (type == typeof(int))
                {
                    defaultValue = 0.ToString();
                }
                else if (type == typeof(float))
                {
                    defaultValue = 0.0f.ToString(CultureInfo.InvariantCulture);
                }
                else if (type == typeof(Vector2))
                {
                    defaultValue = new Vector2().ToString("F2");
                }
                else if (type == typeof(Vector3))
                {
                    defaultValue = new Vector3().ToString("F2");
                }
                else if (type.IsEnum)
                {
                    defaultValue = Enum.GetNames(type)[0];
                }
                
                _nodeDataDto.vars.Add(new BTVar
                {
                    id = id,
                    value = defaultValue,
                    typeReference = type.AssemblyQualifiedName
                });
            }

            string GetNodeVarValue(string id)
            {
                return _nodeDataDto.vars.FirstOrDefault(var => var.id == id)?.value;
            }

            void SetNodeVarValue(string id, string value, Type type)
            {
                var btVar = _nodeDataDto.vars.FirstOrDefault(var => var.id == id);
                if (btVar != null)
                {
                    btVar.value = value;
                }
                else
                {
                    btVar = new BTVar
                    {
                        id = id,
                        value = value,
                        typeReference = type.AssemblyQualifiedName
                    };
                    _nodeDataDto.vars.Add(btVar);
                }
            }

            void AddSeparator()
            {
                var separator = new VisualElement
                {
                    style =
                    {
                        height = 1, // Set the height for a horizontal line
                        backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1), // Set color for the line
                        marginTop = 4,
                        marginBottom = 4,
                        flexGrow = 1 // Make it expand horizontally
                    }
                };
                Add(separator);
            }
        }
    }
}