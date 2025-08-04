using System;

namespace ShadyMax.DialogSystem.Editor
{
    [Serializable]
    public class BlackboardVariable
    {
        public string name;
        public string type;
        public string value;

        public BlackboardVariable(string name, string type, string value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }
    }
}