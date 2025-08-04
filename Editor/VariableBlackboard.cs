using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor
{
    public class VariableBlackboard : Blackboard
    {
        public VariableBlackboard(GraphView graphView) : base(graphView)
        {
            addItemRequested = OnAddItemRequested;
            
        }

        private void OnAddItemRequested(Blackboard blackboard)
        {
            var variableSearchWindow = ScriptableObject.CreateInstance<VariableSearchWindow>();
            variableSearchWindow.Initialize(this);
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), variableSearchWindow);
        }
    }
}