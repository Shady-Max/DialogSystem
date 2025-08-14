using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor
{
    public class VariableDragManipulator : MouseManipulator
    {
        private string variableGuid;
        private Vector2 startPosition;
    
        public VariableDragManipulator(string guid)
        {
            variableGuid = guid;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }
        
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (CanStartManipulation(evt))
            {
                startPosition = evt.localMousePosition;
                target.CaptureMouse();
                evt.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (target.HasMouseCapture())
            {
                var delta = evt.localMousePosition - startPosition;
                if (delta.magnitude > 10f) // Start drag after minimum distance
                {
                    StartDrag();
                }
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (target.HasMouseCapture())
            {
                target.ReleaseMouse();
            }
        }

        private void StartDrag()
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData("VariableGuid", variableGuid);
            DragAndDrop.StartDrag("Variable");
        }
    }
}