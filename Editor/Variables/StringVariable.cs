using System;

namespace ShadyMax.DialogSystem.Editor.Variables
{
    [Serializable]
    public class StringVariable : BaseVariable
    {
        public string value;
        
        public StringVariable(string name, string type, string value) : base(name, type, value)
        {
            this.value = value;
        }
        
        public override void OnBeforeSerialize()
        {
            stringValue = value ?? "";
        }
        
        protected override void RestoreFromStringValue()
        {
            value = stringValue ?? "";
        }
    }
}