using System;
using System.Collections.Generic;
using ShadyMax.DialogSystem.Editor.Nodes;
using ShadyMax.DialogSystem.Editor.Variables;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor.ViewNodes
{
    public class VariableSetNodeView : BaseNodeView<VariableSetNodeEditor>
    {
        public CustomPort inputPort;
        public bool portConnected = false;
        
        public override void Initialize(BaseNodeEditor node, DialogGraphView dialogGraphView)
        {
            base.Initialize(node, dialogGraphView);
            this.schedule.Execute(() => {
                CheckPortConnection();
            }).ExecuteLater(100);
        }
        
        private void CheckPortConnection()
        {
            if (inputPort != null)
            {
                bool wasConnected = portConnected;
                portConnected = inputPort.connected;
        
                if (wasConnected != portConnected)
                {
                    RefreshUI();
                }
            }
        }
        
        protected override string GetTitle() => "Variable Set Node";

        protected override void CreateInputPorts()
        {
            if (inputPort == null)
            {
                inputPort = CustomPort.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(BaseNodeEditor));
                inputPort.portName = "In";
                inputPort.name = "In";
                inputContainer.Add(inputPort);
                
                inputPort.OnPortConnect += (port, edge) =>
                {
                    portConnected = true;
                    RefreshUI();
                };
                
                inputPort.OnPortDisconnect += (port, edge) =>
                {
                    portConnected = false;
                    RefreshUI();
                };
            }
        }

        protected override void CreateOutputPorts() { }

        protected override void CreateContent()
        {
            extensionContainer.Clear();
            var variables = GraphView.DialogReference.variables;
            var variableNames = new List<string>();
            foreach (var variable in variables)
            {
                variableNames.Add(variable.name);
            }
            if (variableNames.Count == 0)
            {
                variableNames.Add("---");
                inputPort.ClearAllowedDataTypes();
                inputPort.AddAllowedDataType<BaseNodeEditor>();
                inputPort.portType = typeof(BaseNodeEditor);
            }
            else
            {
                var variable = variables.Find(var => var.guid == node.variableGuid);
                if (variable == null)
                {
                    Undo.RecordObject(node, "Change variable Guid");
                    node.variableGuid = variables[0].guid;
                    EditorUtility.SetDirty(node);
                    GraphView.GraphChanged?.Invoke();
                }
                int currentIndex = string.IsNullOrEmpty(node.variableGuid) ? 0 : variables.FindIndex(v => v.guid == node.variableGuid);
                DropdownField dropdown = new DropdownField("Variable", variableNames, currentIndex);
                inputPort.portType = variable.type switch
                {
                    "int" => typeof(int),
                    "float" => typeof(float),
                    "bool" => typeof(bool),
                    "string" => typeof(string),
                    _ => null
                };
                inputPort.ClearAllowedDataTypes();
                inputPort.AddAllowedDataType(inputPort.portType);
                if (inputPort.portType == typeof(int)) 
                    inputPort.AddAllowedDataType<float>();
                else if (inputPort.portType == typeof(float))
                    inputPort.AddAllowedDataType<int>();
                RemoveIncompatibleEdges();
                dropdown.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(node, "Change variable Guid");
                    var variable = GraphView.DialogReference.variables.Find(v => v.name == evt.newValue);
                    node.variableGuid = variable?.guid;
                    inputPort.portType = variable.type switch
                    {
                        "int" => typeof(int),
                        "float" => typeof(float),
                        "bool" => typeof(bool),
                        "string" => typeof(string),
                        _ => null
                    };
                    inputPort.ClearAllowedDataTypes();
                    inputPort.AddAllowedDataType(inputPort.portType);
                    if (inputPort.portType == typeof(int)) 
                        inputPort.AddAllowedDataType<float>();
                    else if (inputPort.portType == typeof(float))
                        inputPort.AddAllowedDataType<int>();
                    RemoveIncompatibleEdges();
                    EditorUtility.SetDirty(node);
                    GraphView.GraphChanged?.Invoke();
                    RefreshUI();
                });
                
                extensionContainer.Add(dropdown);
            }
        }

        private void RemoveIncompatibleEdges()
        {
            if (inputPort == null) return;

            var edgesToRemove = new List<Edge>();

            foreach (Edge edge in inputPort.connections)
            {
                if (edge.output != null && !inputPort.IsConnectionAllowed(edge.output))
                {
                    edgesToRemove.Add(edge);
                }
            }

            foreach (Edge edge in edgesToRemove)
            {
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                GraphView.RemoveElement(edge);
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