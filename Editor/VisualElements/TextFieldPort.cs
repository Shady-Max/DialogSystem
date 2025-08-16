using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor.VisualElements
{
    public class TextFieldPort : FieldPort<string, TextField>
    {
        public TextFieldPort(string portName, Direction direction, Port.Capacity capacity) : base(portName, direction, capacity)
        {
            field.labelElement.style.minWidth = 40;
            field.label = "Value";
        }
    }
}