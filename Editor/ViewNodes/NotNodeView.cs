using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor.Experimental.GraphView;

namespace ShadyMax.DialogSystem.Editor.ViewNodes
{
    public class NotNodeView : BaseNodeView<NotNodeEditor>
    {
        public CustomPort inputPort;
        public CustomPort outputPort;
        protected override string GetTitle() => "Not Node";

        protected override void CreateInputPorts()
        {
            if (inputPort == null)
            {
                inputPort = CustomPort.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                    typeof(bool));
                inputPort.portName = "In";
                inputPort.name = "In";
                inputContainer.Add(inputPort);
            }
        }

        protected override void CreateOutputPorts()
        {
            if (outputPort == null)
            {
                outputPort = CustomPort.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                    typeof(bool));
                outputPort.portName = "Out";
                outputPort.name = "Out";
                outputContainer.Add(outputPort);
            }
        }

        protected override void CreateContent()
        {
            
        }

        protected override void RefreshUI()
        {
            
        }
    }
}