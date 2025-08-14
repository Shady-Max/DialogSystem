using System;
using System.Collections.Generic;
using System.Globalization;
using ShadyMax.DialogSystem.Editor.Variables;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor
{
    public class VariableSearchWindow: ScriptableObject, ISearchWindowProvider
    {
        private Blackboard _blackboard;
        private DialogGraphView _graphView;

        public void Initialize(Blackboard blackboard, DialogGraphView graphView)
        {
            _blackboard = blackboard;
            _graphView = graphView;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Variables"), 0),
                new SearchTreeEntry(new GUIContent("Int")) {level = 1, userData = "int"},
                new SearchTreeEntry(new GUIContent("Float")) {level = 1, userData = "float"},
                new SearchTreeEntry(new GUIContent("Bool")) {level = 1, userData = "bool"},
                new SearchTreeEntry(new GUIContent("String")) {level = 1, userData = "string"},
            };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var variableType = searchTreeEntry.userData as string;
            if (string.IsNullOrEmpty(variableType)) return false;

            var guid = Guid.NewGuid().ToString();
            BaseVariable variable = variableType switch
            {
                "int" => new IntVariable(Guid.NewGuid().ToString(), "int", 0) { guid = guid },
                "float" => new FloatVariable(Guid.NewGuid().ToString(), "float", 0.0f) { guid = guid },
                "bool" => new BoolVariable(Guid.NewGuid().ToString(), "bool", false) { guid = guid },
                "string" => new StringVariable(Guid.NewGuid().ToString(), "string", "") { guid = guid },
                _ => new BaseVariable(Guid.NewGuid().ToString(), "Invalid type", "") { guid = guid }
            };

            if (variable != null)
            {
                _graphView.OnVariableAdded(variable);
                return true;
            }

            return false;
        }
    }
}