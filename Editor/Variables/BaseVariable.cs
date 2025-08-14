using System;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor.Variables
{
    [Serializable]
    public class BaseVariable : ISerializationCallbackReceiver
    {
        public string name;
        public string type;
        public string stringValue;
        public string guid;

        public BaseVariable(string name, string type, string value)
        {
            this.name = name;
            this.type = type;
            this.stringValue = value;
        }
        
        public virtual void OnBeforeSerialize()
        {
            // Override in derived classes if needed
        }

        public virtual void OnAfterDeserialize()
        {
            RestoreFromStringValue();
        }

        // Virtual method for derived classes to implement
        protected virtual void RestoreFromStringValue()
        {
            // Base implementation does nothing
        }
    }
}