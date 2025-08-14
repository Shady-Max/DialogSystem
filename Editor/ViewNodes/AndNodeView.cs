using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor.Experimental.GraphView;

namespace ShadyMax.DialogSystem.Editor.ViewNodes
{
    public class AndNodeView : BaseNodeView<AndNodeEditor>
    {
        public CustomPort inputPort1;
        public CustomPort inputPort2;
        public CustomPort outputPort;
        
        protected override string GetTitle() => "And Node";

        protected override void CreateInputPorts()
        {
            if (inputPort1 == null)
            {
                inputPort1 = CustomPort.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                    typeof(bool));
                inputPort1.portName = "In 1";
                inputPort1.name = "In 1";
                inputContainer.Add(inputPort1);
            }
            if (inputPort2 == null)
            {
                inputPort2 = CustomPort.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                    typeof(bool));
                inputPort2.portName = "In 2";
                inputPort2.name = "In 2";
                inputContainer.Add(inputPort2);
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

        protected override void CreateContent() { }

        protected override void RefreshUI() { }
    }
}