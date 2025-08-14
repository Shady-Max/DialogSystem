using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor;

namespace ShadyMax.DialogSystem.Editor.InspectorEditor
{
    [CustomEditor(typeof(BeginNodeEditor))]
    public class BeginNodeInspector : BaseNodeInspector<BeginNodeEditor>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI(); 
        }
    }
}