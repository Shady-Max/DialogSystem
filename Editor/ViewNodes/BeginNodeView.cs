using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor.Experimental.GraphView;

namespace ShadyMax.DialogSystem.Editor.ViewNodes
{
    public class BeginNodeView : BaseNodeView<BaseNodeEditor>
    {
        public Port outputPort;
        protected override string GetTitle() => "Begin Node";

        protected override void CreateInputPorts()
        {
            
        }

        protected override void CreateOutputPorts()
        {
            if (outputPort != null)
                return;
                
            outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BaseNodeEditor));
            outputPort.portName = "Out";
            outputPort.name = "Out";
            outputContainer.Add(outputPort);
        }

        protected override void CreateContent()
        {
            
        }

        protected override void RefreshUI()
        {
            
        }
    }
}