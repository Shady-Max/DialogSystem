using ShadyMax.DialogSystem.Editor.Nodes;

namespace ShadyMax.DialogSystem.Editor.InspectorEditor
{
    public class BaseNodeChildInspector<T> : BaseNodeInspector where T : BaseNodeEditor
    {
        protected T _target;
        protected new void OnEnable()
        {
            base.OnEnable();
            _target = target as T;
        }
    }
}