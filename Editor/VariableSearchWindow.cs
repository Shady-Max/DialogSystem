using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor
{
    public class VariableSearchWindow: ScriptableObject, ISearchWindowProvider
    {
        private Blackboard _blackboard;

        public void Initialize(Blackboard blackboard)
        {
            _blackboard = blackboard;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Variables"), 0),
                new SearchTreeEntry(new GUIContent("Int")) {level = 1, userData = new BlackboardVariable(Guid.NewGuid().ToString(),"int","0")},
                new SearchTreeEntry(new GUIContent("Float")) {level = 1, userData = new BlackboardVariable(Guid.NewGuid().ToString(),"float","0.0")},
                new SearchTreeEntry(new GUIContent("Bool")) {level = 1, userData = new BlackboardVariable(Guid.NewGuid().ToString(),"bool","false")},
                new SearchTreeEntry(new GUIContent("String")) {level = 1, userData = new BlackboardVariable(Guid.NewGuid().ToString(),"string","")},
            };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData is BlackboardVariable variable)
            {
                var field = new BlackboardField(){text = variable.name, typeText = variable.type};
                //TODO: make it read data
                var row = variable.type switch
                {
                    "int" => new BlackboardRow(field, new IntegerField("value") { value = int.Parse(variable.value) }),
                    "float" => new BlackboardRow(field,
                        new FloatField("value") { value = float.Parse(variable.value, NumberStyles.Float, CultureInfo.InvariantCulture) }),
                    "bool" => new BlackboardRow(field, new Toggle("value") { value = bool.Parse(variable.value) }),
                    "string" => new BlackboardRow(field, new TextField("value") { value = variable.value }),
                    _ => new BlackboardRow(field, new Label("Invalid type"))
                };
                _blackboard.Add(row);
                return true;
            }
            return false;
        }
    }
}