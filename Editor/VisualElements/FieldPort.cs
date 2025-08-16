using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor.VisualElements
{
    public class FieldPort<T, T1> : VisualElement 
        where T1 : VisualElement, new()
    {
        public T1 field;
        public CustomPort port;
        
        public FieldPort(string portName, Direction direction, Port.Capacity capacity)
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.FlexStart;
            
            port = CustomPort.Create<Edge>(Orientation.Horizontal, direction, capacity, typeof(T));
            port.portName = portName;
            port.name = portName;
            
            field = new T1();
            field.style.minWidth = 100;

            if (direction == Direction.Input)
            {
                Add(port);
                Add(field);
            }
            else
            {
                Add(field);
                Add(port);
            }
        }
    }
}