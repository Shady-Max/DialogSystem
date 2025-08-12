using UnityEditor;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor.Nodes
{
    public class BaseNodeEditor : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private string _guid;
        [SerializeField] private Vector2 _position;
        
        public string tableReference;
        public System.Action OnDataChanged;

        
        public string Guid
        {
            get => _guid;
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                _guid = value;
            }
        }

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
                    // Extract GUID from the deleted name
                    _guid = name.Substring("DELETED_".Length);
                    name = $"{GetType().Name}_{_guid}";
                    EditorUtility.SetDirty(this);

                }
                else
                {
                    _guid = System.Guid.NewGuid().ToString();
                    name = $"{GetType().Name}_{_guid}";


                }
                
            } else if (name != null && name.StartsWith("DELETED_"))
            {
                // This handles the case when guid exists but name is still in deleted state
                name = $"{GetType().Name}_{_guid}";
                EditorUtility.SetDirty(this);
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