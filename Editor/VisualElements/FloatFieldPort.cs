using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor.VisualElements
{
    public class FloatFieldPort : FieldPort<float, FloatField>
    {
        public FloatFieldPort(string portName, Direction direction, Port.Capacity capacity) : base(portName, direction, capacity)
        {
            field.labelElement.style.minWidth = 40;
            field.label = "Value";
            field.value = 0;
        }
    }
}