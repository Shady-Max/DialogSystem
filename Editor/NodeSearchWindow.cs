using System;
using System.Collections.Generic;
using ShadyMax.DialogSystem.Editor.Nodes;
using ShadyMax.DialogSystem.Editor.Variables;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogGraphView _graphView;

        public void Initialize(DialogGraphView graphView)
        {
            _graphView = graphView;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                 new SearchTreeGroupEntry(new GUIContent("Create Node")),
                 
                 new SearchTreeGroupEntry(new GUIContent("Dialog Nodes"), 1),
                 new SearchTreeEntry(new GUIContent("Sentence Node")) {level = 2, userData = typeof(SentenceNodeEditor)},
                 new SearchTreeEntry(new GUIContent("Answer Node")) {level = 2, userData = typeof(AnswerNodeEditor)},
                 
                 new SearchTreeGroupEntry(new GUIContent("Logic"), 1),
                 new SearchTreeEntry(new GUIContent("And")) {level = 2, userData = typeof(AndNodeEditor)},
                 new SearchTreeEntry(new GUIContent("Or")) {level = 2, userData = typeof(OrNodeEditor)},
                 new SearchTreeEntry(new GUIContent("Not")) {level = 2, userData = typeof(NotNodeEditor)},
                 new SearchTreeEntry(new GUIContent("If")) {level = 2, userData = typeof(IfNodeEditor)},
                 
                 new SearchTreeGroupEntry(new GUIContent("Math"), 1),
                 //new SearchTreeEntry(new GUIContent("Add")) {level = 2, userData = typeof(AddNodeEditor)},
                 //new SearchTreeEntry(new GUIContent("Subtract")) {level = 2, userData = typeof(SubtractNodeEditor)},
                 //new SearchTreeEntry(new GUIContent("Multiply")) {level = 2, userData = typeof(MultiplyNodeEditor)},
                 //new SearchTreeEntry(new GUIContent("Divide")) {level = 2, userData = typeof(DivideNodeEditor)},
                 //new SearchTreeEntry(new GUIContent("Set")) {level = 2, userData = typeof(SetNodeEditor)},
                 
                 new SearchTreeGroupEntry(new GUIContent("Value"), 1),
                 //new SearchTreeEntry(new GUIContent("Float")) {level = 2, userData = typeof(FloatNodeEditor)},
                 //new SearchTreeEntry(new GUIContent("Int")) {level = 2, userData = typeof(IntNodeEditor)},
                 //new SearchTreeEntry(new GUIContent("String")) {level = 2, userData = typeof(StringNodeEditor)},
                 //new SearchTreeEntry(new GUIContent("Bool")) {level = 2, userData = typeof(BoolNodeEditor)},
                 
                 new SearchTreeGroupEntry(new GUIContent("Variables"), 1),
                 /*
                  new SearchTreeEntry(new GUIContent("Set Variable")) {level = 2, userData = typeof(BaseNodeEditor)},
                  */
            };

            foreach (var variable in _graphView.DialogReference.variables)
            {
                tree.Add(new SearchTreeEntry(new GUIContent(variable.name)) {level = 2, userData = variable});
            }
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if (SearchTreeEntry.userData is Type nodeType && 
                (nodeType.IsSubclassOf(typeof(BaseNodeEditor)) || nodeType == typeof(BaseNodeEditor)))
            {
                _graphView.CreateNode(nodeType);
                return true;
            } 
            if (SearchTreeEntry.userData is BaseVariable variable)
            {
                Rect worldRect = _graphView.layout;
                worldRect.position = Vector2.zero;

                // Middle point in local coordinates
                Vector2 localCenter = worldRect.center;
                Vector2 position = _graphView.contentViewContainer.WorldToLocal(_graphView.LocalToWorld(localCenter));
                
                _graphView.CreateVariableGetNode(variable.guid, position);
                return true;
            }
            return false;
        }
    }
}