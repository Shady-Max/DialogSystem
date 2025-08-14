using System.Collections.Generic;
using System.Globalization;
using ShadyMax.DialogSystem.Editor.Variables;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor
{
    public class VariableBlackboard : Blackboard
    {
        private DialogGraphView _graphView;
        public VariableBlackboard(DialogGraphView graphView) : base(graphView)
        {
            _graphView = graphView;
            title = "Variables";
            addItemRequested = OnAddItemRequested;
        }
        
        public void LoadVariables(List<BaseVariable> variables)
        {
            Clear();
            var variablesCopy = new List<BaseVariable>(variables);
            foreach (var variable in variablesCopy)
            {
                variable.OnAfterDeserialize();
                AddVariableField(variable);
            }
        }
        
        public void AddVariableField(BaseVariable variable)
        {
            BaseVariable typedVariable = variable.type switch
            {
                "int" => new IntVariable(variable.name, variable.type, 0) { guid = variable.guid, stringValue = variable.stringValue },
                "float" => new FloatVariable(variable.name, variable.type, 0.0f) { guid = variable.guid, stringValue = variable.stringValue },
                "bool" => new BoolVariable(variable.name, variable.type, false) { guid = variable.guid, stringValue = variable.stringValue },
                "string" => new StringVariable(variable.name, variable.type, "") { guid = variable.guid, stringValue = variable.stringValue },
                _ => variable
            };
            
            typedVariable.OnAfterDeserialize();
            
            var variableIndex = _graphView.DialogReference.variables.FindIndex(v => v.guid == variable.guid);
            if (variableIndex >= 0)
            {
                _graphView.DialogReference.variables[variableIndex] = typedVariable;
            }
            
            var field = new BlackboardField
            {
                text = typedVariable.name,
                typeText = typedVariable.type
            };
            
            field.capabilities &= ~Capabilities.Renamable;
            
            field.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2 && evt.button == 0) // Double left click
                {
                    StartEditingFieldName(field, typedVariable);
                    evt.StopPropagation();
                    evt.PreventDefault();
                }
                else if (evt.button == 0) // Left click
                {
                    field.CaptureMouse();
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.SetGenericData("BlackboardField", typedVariable); // store reference
                }
            });
            
            field.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (!field.HasMouseCapture()) return;

                DragAndDrop.StartDrag("Dragging from Blackboard");
                field.ReleaseMouse();
            });
            
            var valueField = typedVariable.type switch
            {
                "int" => CreateIntField(typedVariable as IntVariable),
                "float" => CreateFloatField(typedVariable as FloatVariable),
                "bool" => CreateBoolField(typedVariable as BoolVariable),
                "string" => CreateStringField(typedVariable as StringVariable),
                _ => new Label("Invalid type") as VisualElement
            };

            var row = new BlackboardRow(field, valueField);
            field.userData = typedVariable;

            // Add context menu for deletion
            field.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
            {
                evt.menu.AppendAction("Rename", action =>
                {
                    StartEditingFieldName(field, typedVariable);
                });
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Delete", action =>
                {
                    Remove(row);
                    _graphView.OnVariableRemoved(typedVariable.guid);
                });
                evt.StopPropagation();
            });

            Add(row);
        }
        
        private IntegerField CreateIntField(IntVariable intVar)
        {
            var field = new IntegerField("value") { value = intVar?.value ?? 0 };
            field.RegisterValueChangedCallback(evt =>
            { 
                if (intVar != null)
                {
                    intVar.value = evt.newValue;
                    intVar.OnBeforeSerialize();
                    _graphView.SaveVariables(_graphView.DialogReference.variables);
                }
            });
            return field;
        }
        
        private FloatField CreateFloatField(FloatVariable floatVar)
        {
            var field = new FloatField("value") { value = floatVar?.value ?? 0.0f };
            field.RegisterValueChangedCallback(evt =>
            {
                if (floatVar != null)
                {
                    floatVar.value = evt.newValue;
                    floatVar.OnBeforeSerialize();
                    _graphView.SaveVariables(_graphView.DialogReference.variables);
                }
            });
            return field;
        }
        
        private Toggle CreateBoolField(BoolVariable boolVar)
        {
            var field = new Toggle("value") { value = boolVar?.value ?? false };
            field.RegisterValueChangedCallback(evt =>
            {
                if (boolVar != null)
                {
                    boolVar.value = evt.newValue;
                    boolVar.OnBeforeSerialize();
                    _graphView.SaveVariables(_graphView.DialogReference.variables);
                }
            });
            return field;
        }
        
        private TextField CreateStringField(StringVariable stringVar)
        {
            var field = new TextField("value") { value = stringVar?.value ?? "" };
            field.RegisterValueChangedCallback(evt =>
            {
                if (stringVar != null)
                {
                    stringVar.value = evt.newValue;
                    stringVar.OnBeforeSerialize();
                    _graphView.SaveVariables(_graphView.DialogReference.variables);
                }
            });
            return field;
        }
        
        private void StartEditingFieldName(BlackboardField field, BaseVariable variable)
        {
            var textField = new TextField
            {
                value = variable.name,
            };
 
            // Create an overlay container that sits on top of the field
            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);

            textField.style.position = Position.Absolute;
            textField.style.left = 5;
            textField.style.top = 2;
            textField.style.right = 5;
            textField.style.bottom = 2;
            textField.style.fontSize = field.resolvedStyle.fontSize;
            
            overlay.Add(textField);
            field.Add(overlay);
            
            var originalText = field.text;
            field.text = "";
            textField.Focus();
            textField.SelectAll();
            
            System.Action finishEditing = () =>
            {
                string newName = ValidateAndGetUniqueName(textField.value, variable.guid);
                variable.name = newName;
                field.text = newName;
                field.Remove(overlay);
                _graphView.SaveVariables(_graphView.DialogReference.variables);
                _graphView.LoadGraph(_graphView.DialogReference, true);
            };
            
            System.Action cancelEditing = () =>
            {
                field.Remove(overlay);
            };

            textField.RegisterCallback<FocusOutEvent>(evt => finishEditing());
            textField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    finishEditing();
                    evt.StopPropagation();
                }
                else if (evt.keyCode == KeyCode.Escape)
                {
                    cancelEditing();
                    evt.StopPropagation();
                }
            });
        }
        
        private string ValidateAndGetUniqueName(string desiredName, string excludeGuid)
        {
            if (string.IsNullOrWhiteSpace(desiredName))
                desiredName = "Variable";

            desiredName = desiredName.Trim();

            var existingVariables = _graphView.DialogReference?.variables;
            if (existingVariables == null) return desiredName;

            bool nameExists = existingVariables.Exists(v =>
                v.guid != excludeGuid &&
                string.Equals(v.name, desiredName, System.StringComparison.OrdinalIgnoreCase));

            if (!nameExists)
                return desiredName;

            int counter = 1;
            string baseName = desiredName;

            var match = System.Text.RegularExpressions.Regex.Match(desiredName, @"^(.+?)(\d+)$");
            if (match.Success)
            {
                baseName = match.Groups[1].Value;
                if (int.TryParse(match.Groups[2].Value, out int existingNumber))
                {
                    counter = existingNumber;
                }
            }

            string uniqueName;
            do
            {
                uniqueName = $"{baseName}{counter}";
                counter++;
            } while (existingVariables.Exists(v =>
                         v.guid != excludeGuid &&
                         string.Equals(v.name, uniqueName, System.StringComparison.OrdinalIgnoreCase)));

            return uniqueName;
        }

        private void OnAddItemRequested(Blackboard blackboard)
        {
            var variableSearchWindow = ScriptableObject.CreateInstance<VariableSearchWindow>();
            variableSearchWindow.Initialize(this, _graphView);
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), variableSearchWindow);
        }
    }
}