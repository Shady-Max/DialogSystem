using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor.VisualElements
{
    public class ToggleFieldPort : FieldPort<bool, Toggle>
    {
        public ToggleFieldPort(string portName, Direction direction, Port.Capacity capacity) : base(portName, direction, capacity)
        {
            field.labelElement.style.minWidth = 40;
            field.label = "Value";
            field.value = false;
        }
    }
}