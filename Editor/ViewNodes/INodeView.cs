using ShadyMax.DialogSystem.Editor.Nodes;

namespace ShadyMax.DialogSystem.Editor.ViewNodes
{
    public interface INodeView
    {
        void Initialize(BaseNodeEditor node, DialogGraphView graphView);
    }
}