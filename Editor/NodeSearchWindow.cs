using System;
using System.Collections.Generic;
using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
                 /*new SearchTreeEntry(new GUIContent("And")) {level = 2, userData = typeof(BaseNodeEditor)},
                 new SearchTreeEntry(new GUIContent("Or")) {level = 2, userData = typeof(BaseNodeEditor)},
                 new SearchTreeEntry(new GUIContent("If")) {level = 2, userData = typeof(BaseNodeEditor)},*/
                 
                 new SearchTreeGroupEntry(new GUIContent("Math"), 1),
                 /*new SearchTreeEntry(new GUIContent("Add")) {level = 2, userData = typeof(BaseNodeEditor)},
                 new SearchTreeEntry(new GUIContent("Subtract")) {level = 2, userData = typeof(BaseNodeEditor)},
                 new SearchTreeEntry(new GUIContent("Multiply")) {level = 2, userData = typeof(BaseNodeEditor)},
                 new SearchTreeEntry(new GUIContent("Divide")) {level = 2, userData = typeof(BaseNodeEditor)},
                 new SearchTreeEntry(new GUIContent("Set")) {level = 2, userData = typeof(BaseNodeEditor)},*/
                 
                 new SearchTreeGroupEntry(new GUIContent("Value"), 1),
                 /*new SearchTreeEntry(new GUIContent("Float")) {level = 2, userData = typeof(BaseNodeEditor)},
                 new SearchTreeEntry(new GUIContent("Int")) {level = 2, userData = typeof(BaseNodeEditor)},
                 new SearchTreeEntry(new GUIContent("String")) {level = 2, userData = typeof(BaseNodeEditor)},
                 new SearchTreeEntry(new GUIContent("Bool")) {level = 2, userData = typeof(BaseNodeEditor)},*/
            };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if (SearchTreeEntry.userData is Type nodeType)
            {
                _graphView.CreateNode(nodeType);
                return true;
            }

            return false;
        }
    }
}