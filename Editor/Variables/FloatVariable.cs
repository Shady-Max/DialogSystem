using System;
using System.Globalization;

namespace ShadyMax.DialogSystem.Editor.Variables
{
    [Serializable]
    public class FloatVariable : BaseVariable
    {
        public float value;
        
        public FloatVariable(string name, string type, float value) : base(name, type, value.ToString(CultureInfo.InvariantCulture))
        {
            this.value = value;
        }
        
        public override void OnBeforeSerialize()
        {
            stringValue = value.ToString(CultureInfo.InvariantCulture);
        }
        
        protected override void RestoreFromStringValue()
        {
            if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
            {
                value = parsedValue;
            }
        }
    }
}