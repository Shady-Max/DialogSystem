using ShadyMax.DialogSystem.Editor.Nodes;
using ShadyMax.DialogSystem.Editor.VisualElements;
using UnityEditor.Experimental.GraphView;

namespace ShadyMax.DialogSystem.Editor.ViewNodes
{
    public class FloatNodeView : BaseNodeView<FloatNodeEditor>
    {
        public FloatFieldPort valuePort;
        protected override string GetTitle() => "Float Node";

        protected override void CreateInputPorts() { }

        protected override void CreateOutputPorts()
        {
            if (valuePort == null)
            {
                valuePort = new FloatFieldPort("Out", Direction.Output, Port.Capacity.Multi);
                outputContainer.Add(valuePort);
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