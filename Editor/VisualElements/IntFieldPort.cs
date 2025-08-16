using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor.VisualElements
{
    public class IntFieldPort : FieldPort<int, IntegerField>
    {
        public IntFieldPort(string portName, Direction direction, Port.Capacity capacity) : base(portName, direction, capacity)
        {
            field.labelElement.style.minWidth = 40;
            field.label = "Value";
            field.value = 0;
        }
    }
}