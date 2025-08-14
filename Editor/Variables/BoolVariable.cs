using System;

namespace ShadyMax.DialogSystem.Editor.Variables
{
    [Serializable]
    public class BoolVariable : BaseVariable
    {
        public bool value;
        
        public BoolVariable(string name, string type, bool value) : base(name, type, value.ToString())
        {
            this.value = value;
        }
        
        public override void OnBeforeSerialize()
        {
            stringValue = value.ToString();
        }
         
        protected override void RestoreFromStringValue()
        {
            if (bool.TryParse(stringValue, out bool parsedValue))
            {
                value = parsedValue;
            }
        }
    }
}