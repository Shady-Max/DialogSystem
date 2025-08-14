using System;

namespace ShadyMax.DialogSystem.Editor.Variables
{
	[Serializable]
    public class IntVariable : BaseVariable
    {
        public int value;

        public IntVariable(string name, string type, int value) : base(name, type, value.ToString())
        {
            this.value = value;
        }
        
        public override void OnBeforeSerialize()
        {
            stringValue = value.ToString();
        }
        
        protected override void RestoreFromStringValue()
        {
            if (int.TryParse(stringValue, out int parsedValue))
            {
                value = parsedValue;
            }
        }
    }
}