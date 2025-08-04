using UnityEditor;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor.Nodes
{
    public class BaseNodeEditor : ScriptableObject, ISerializationCallbackReceiver
    {
        private string _guid;
        private Vector2 _position;
        
        public string tableReference;
        public System.Action OnDataChanged;

        
        public string Guid { get => _guid; set => _guid = value; }
        public Vector2 Position { get => _position; set => _position = value; }
        
        private void OnEnable()
        {
            Initialize();
        }
        
        public void InitializeLocalization(string table)
        {
            tableReference = table;
        }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(_guid))
            {
                if (name != null && name.StartsWith("DELETED_"))
                {
                    _guid = name.Substring("DELETED_".Length);
                    name = $"{GetType().Name}_{_guid}";
                }
                else
                {
                    _guid = System.Guid.NewGuid().ToString();
                }
                
            }
                
        }
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            EditorApplication.delayCall += () =>
            {
                OnDataChanged?.Invoke();
            };

        }
    }
}