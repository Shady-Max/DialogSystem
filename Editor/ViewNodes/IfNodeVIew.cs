using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ShadyMax.DialogSystem.Editor.Nodes;
using ShadyMax.DialogSystem.Editor.Variables;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;

namespace ShadyMax.DialogSystem.Editor.ViewNodes
{
    public class IfNodeVIew : BaseNodeView<IfNodeEditor>
    {
        public Port inputPort;
        public CustomPort conditionPort;
        public bool conditionPortConnected = false;
        public Port truePort;
        public Port falsePort;
        public CustomPort valuePort;
        
        protected override string GetTitle() => "If Node";
        
        private readonly List<string> numberConditions = new List<string>() { "Equals", "Not Equals", "Greater Than", "Less Than", "Greater Equal Than", "Less Equal Than" };
        private readonly List<string> otherConditions = new List<string>() { "Equals", "Not Equals" };

        public override void Initialize(BaseNodeEditor node, DialogGraphView dialogGraphView)
        {
            base.Initialize(node, dialogGraphView);
            this.schedule.Execute(() => {
                CheckConditionPortConnection();
            }).ExecuteLater(100);
        }
        
        private void CheckConditionPortConnection()
        {
            if (conditionPort != null)
            {
                bool wasConnected = conditionPortConnected;
                conditionPortConnected = conditionPort.connected;
        
                if (wasConnected != conditionPortConnected)
                {
                    RefreshUI();
                }
            }
        }
        
        protected override void CreateInputPorts()
        {
            if (inputPort == null)
            {
                inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi,
                    typeof(BaseNodeEditor));
                inputPort.portName = "In";
                inputPort.name = "In";
                inputContainer.Add(inputPort);
            }
            if (conditionPort == null)
            {
                conditionPort = CustomPort.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                    typeof(bool));
                conditionPort.AddAllowedDataType<int>();
                conditionPort.AddAllowedDataType<float>();
                conditionPort.AddAllowedDataType<string>();
                conditionPort.portName = "Condition Value";
                conditionPort.name = "Condition Value";

                conditionPort.OnPortConnect += (port, edge) =>
                {
                    conditionPortConnected = true;
                    Undo.RecordObject(node, "Change Variable Guid");
                    node.variableGuid = string.Empty;
                    EditorUtility.SetDirty(node);
                    GraphView.GraphChanged?.Invoke();
                    RefreshUI();
                };
                conditionPort.OnPortDisconnect += (port, edge) =>
                {
                    conditionPortConnected = false;
                    RefreshUI();
                };
                inputContainer.Add(conditionPort);
            }
        }

        protected override void CreateOutputPorts()
        {
            if (valuePort == null)
            {
                valuePort = CustomPort.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                    typeof(bool));
                valuePort.portName = "Value";
                valuePort.name = "Value Out";
                outputContainer.Add(valuePort);
            }
            if (truePort == null)
            {
                truePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                    typeof(BaseNodeEditor));
                truePort.portName = "True";
                truePort.name = "True Out";
                outputContainer.Add(truePort);
            }
            if (falsePort == null)
            {
                falsePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                    typeof(BaseNodeEditor));
                falsePort.portName = "False";
                falsePort.name = "False Out";
                outputContainer.Add(falsePort);
            }
        }

        protected override void CreateContent()
        {
            extensionContainer.Clear();
            Type variableType = null;
            if (!conditionPortConnected)
            {
                List<string> variableNames = new List<string>();
                foreach (var variable in GraphView.DialogReference.variables)
                {
                    if (variable is BaseVariable baseVariable)
                    {
                        variableNames.Add(baseVariable.name);
                    }
                }

                if (variableNames.Count == 0)
                {
                    variableNames.Add("---");
                }

                int currentIndex = string.IsNullOrEmpty(node.variableGuid) ? 0 : GraphView.DialogReference.variables.FindIndex(v => v.guid == node.variableGuid); ;
                
                DropdownField dropdownVariable = new DropdownField("Variable", variableNames, currentIndex);
                if (node.variableGuid == string.Empty && variableNames.Count > 0)
                {
                    Undo.RecordObject(node, "Change variable Guid");
                    node.variableGuid = GraphView.DialogReference.variables.Find(v => v.name == variableNames[0])?.guid;
                    EditorUtility.SetDirty(node);
                    GraphView.GraphChanged?.Invoke();
                }
                
                dropdownVariable.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(node, "Change variable Guid");
                    node.variableGuid = GraphView.DialogReference.variables.Find(v => v.name == evt.newValue)?.guid;
                    EditorUtility.SetDirty(node);
                    GraphView.GraphChanged?.Invoke();
                    RefreshUI();
                });
                extensionContainer.Add(dropdownVariable);
            } 
            else
            {
                variableType = conditionPort.connections.First().output.portType;
            }

            if (!string.IsNullOrEmpty(node.variableGuid))
            {
                foreach (var variable in GraphView.DialogReference.variables)
                {
                    if (variable.guid == node.variableGuid)
                    {
                        variableType = variable.type switch
                        {
                            "int" => typeof(int),
                            "float" => typeof(float),
                            "bool" => typeof(bool),
                            "string" => typeof(string),
                            _ => null
                        };
                        break;
                    }
                }
            }

            if (variableType == null)
            {
                DropdownField dropdown = new DropdownField("Condition Type", new List<string>() { "---" }, 0);
                Label label = new Label("Value: none");
                extensionContainer.Add(dropdown);
                extensionContainer.Add(label);
            }
            else if (variableType == typeof(int))
            {
                DropdownField dropdown = new DropdownField("Condition Type", numberConditions, node.conditionType);
                dropdown.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(node, "Change Condition Type");
                    node.conditionType = numberConditions.IndexOf(evt.newValue);
                    EditorUtility.SetDirty(node);
                    GraphView.GraphChanged?.Invoke();
                });
                IntegerField intField = new IntegerField("Value");
                try
                {
                    intField.value = int.Parse(node.value);
                }
                catch (FormatException)
                {
                    Undo.RecordObject(node, "Change Value");
                    node.value = "0";
                    intField.value = 0;
                    EditorUtility.SetDirty(node);
                    GraphView.GraphChanged?.Invoke();
                }
                intField.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(node, "Change Value");
                    node.value = evt.newValue.ToString();
                    EditorUtility.SetDirty(node);
                    GraphView.GraphChanged?.Invoke();
                });
                extensionContainer.Add(dropdown);
                extensionContainer.Add(intField);
            }
            else if (variableType == typeof(float))
            {
                DropdownField dropdown = new DropdownField("Condition Type", numberConditions, 0);
                FloatField intField = new FloatField("Value");
                extensionContainer.Add(dropdown);
                extensionContainer.Add(intField);
            }
            else if (variableType == typeof(bool))
            {
                DropdownField dropdown = new DropdownField("Condition Type", otherConditions, 0);
                Toggle intField = new Toggle("Value");
                extensionContainer.Add(dropdown);
                extensionContainer.Add(intField);
            }
            else if (variableType == typeof(string))
            {
                DropdownField dropdown = new DropdownField("Condition Type", otherConditions, 0);
                TextField intField = new TextField("Value");
                extensionContainer.Add(dropdown);
                extensionContainer.Add(intField);
            }
        }

        protected override void RefreshUI()
        {
            CreateContent();
            RefreshExpandedState();
            RefreshPorts();
        }
    }
}