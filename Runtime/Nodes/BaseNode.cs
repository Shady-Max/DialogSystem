using UnityEngine;

namespace ShadyMax.DialogSystem.Runtime.Nodes
{
    public class BaseNode : ScriptableObject
    { 
        private string _guid;
        
        public string Guid { get => _guid; set => _guid = value; }
        
        private void OnEnable()
        {
            if (string.IsNullOrEmpty(_guid))
                _guid = System.Guid.NewGuid().ToString();
        }
    }
}