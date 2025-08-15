using System;
using ShadyMax.DialogSystem.Editor.Nodes;
using ShadyMax.DialogSystem.Editor.Variables;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor.ViewNodes
{
    public class VariableGetNodeView : BaseNodeView<VariableGetNodeEditor>
    {
        public CustomPort outputPort;
        private Label variableNameLabel;
        private new VisualElement mainContainer;
        private bool isSelected = false;
        protected override string GetTitle() => "";

        protected override void CreateInputPorts() { }

        protected override void CreateOutputPorts()
        {
            if (outputPort == null)
            {
                var variable = GraphView.DialogReference.variables.Find(x => x.guid == node.variableGuid);
                Type type = variable switch
                {
                    IntVariable => typeof(int),
                    FloatVariable => typeof(float),
                    StringVariable => typeof(string),
                    BoolVariable => typeof(bool),
                    _ => typeof(BaseNodeEditor)
                };
                outputPort = CustomPort.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi,
                    type);
                outputPort.portName = "";
                outputPort.name = "Out";
            }
        }

        protected override void CreateContent()
        {
            // Clear all default containers
            titleContainer.Clear();
            topContainer.Clear();
            outputContainer.Clear();
            inputContainer.Clear();
            contentContainer.Clear();
            extensionContainer.Clear();
            
            // Hide title container
            titleContainer.style.display = DisplayStyle.None;
            
            // Create main container for the round node
            mainContainer = new VisualElement();
            mainContainer.style.flexDirection = FlexDirection.Row;
            mainContainer.style.alignItems = Align.Center;
            mainContainer.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.3f, 0.3f, 0.3f, 1f));
            
            mainContainer.style.borderTopWidth = 2;
            mainContainer.style.borderBottomWidth = 2;
            mainContainer.style.borderLeftWidth = 2;
            mainContainer.style.borderRightWidth = 2;
            mainContainer.style.borderTopColor = new StyleColor(new UnityEngine.Color(0, 0, 0, 0));
            mainContainer.style.borderBottomColor = new StyleColor(new UnityEngine.Color(0, 0, 0, 0));
            mainContainer.style.borderLeftColor = new StyleColor(new UnityEngine.Color(0, 0, 0, 0));
            mainContainer.style.borderRightColor = new StyleColor(new UnityEngine.Color(0, 0, 0, 0));
            
            mainContainer.style.borderTopLeftRadius = 15;
            mainContainer.style.borderTopRightRadius = 15;
            mainContainer.style.borderBottomLeftRadius = 15;
            mainContainer.style.borderBottomRightRadius = 15;
            mainContainer.style.paddingLeft = 8;
            mainContainer.style.paddingRight = 8;
            mainContainer.style.paddingTop = 4;
            mainContainer.style.paddingBottom = 4;
            mainContainer.style.minHeight = 30;
            
            mainContainer.RegisterCallback<MouseEnterEvent>(_ =>
            {
                if (!isSelected)
                {
                    var lightBlue = new StyleColor(new UnityEngine.Color(0.3f, 0.7f, 1f, 0.5f));
                    mainContainer.style.borderTopColor = lightBlue;
                    mainContainer.style.borderBottomColor = lightBlue;
                    mainContainer.style.borderLeftColor = lightBlue; 
                    mainContainer.style.borderRightColor = lightBlue;

                    // Make it appear thinner on hover
                    mainContainer.style.borderTopWidth = 1;
                    mainContainer.style.borderBottomWidth = 1;
                    mainContainer.style.borderLeftWidth = 1;
                    mainContainer.style.borderRightWidth = 1;
                }
            });
            
            mainContainer.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                if (!isSelected)
                {
                    var transparent = new StyleColor(new UnityEngine.Color(0, 0, 0, 0));
                    mainContainer.style.borderTopColor = transparent;
                    mainContainer.style.borderBottomColor = transparent;
                    mainContainer.style.borderLeftColor = transparent;
                    mainContainer.style.borderRightColor = transparent;
                }
            });

            var variable = GraphView.DialogReference.variables.Find(x => x.guid == node.variableGuid);
            if (variable == null)
            {
                Debug.LogWarning("Variable not found, destroying variable get node");
                GraphView.DeleteElements(new [] {this});
                return;
            }
            // Variable name field
            variableNameLabel = new Label(variable.name) ;
            variableNameLabel.style.minWidth = 80;
            variableNameLabel.style.fontSize = 12;
            mainContainer.Add(variableNameLabel);
            mainContainer.Add(outputPort);
            
            // Add to the main element
            Add(mainContainer);
            
            // Style the entire node
            style.backgroundColor = StyleKeyword.None;
            style.borderTopWidth = 0;
            style.borderBottomWidth = 0;
            style.borderLeftWidth = 0;
            style.borderRightWidth = 0;
        }
        
        public override void OnSelected()
        {
            base.OnSelected();
            isSelected = true;
            var blue = new StyleColor(new UnityEngine.Color(0f, 0.6f, 1f));
            mainContainer.style.borderTopColor = blue;
            mainContainer.style.borderBottomColor = blue;
            mainContainer.style.borderLeftColor = blue;
            mainContainer.style.borderRightColor = blue;
            
            mainContainer.style.borderTopWidth = 2;
            mainContainer.style.borderBottomWidth = 2;
            mainContainer.style.borderLeftWidth = 2;
            mainContainer.style.borderRightWidth = 2;
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            isSelected = false;
            var transparent = new StyleColor(new UnityEngine.Color(0f, 0f, 0f, 0f));
            mainContainer.style.borderTopColor = transparent;
            mainContainer.style.borderBottomColor = transparent;
            mainContainer.style.borderLeftColor = transparent;
            mainContainer.style.borderRightColor = transparent;
        }

        protected override void RefreshUI()
        {
            
        }
    }
}