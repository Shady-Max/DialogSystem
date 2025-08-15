using System;
using System.Collections.Generic;

namespace ShadyMax.DialogSystem.Editor.Nodes
{
    public class AnswerNodeEditor : BaseNodeEditor
    {
        public int answerCount = 0;
        public bool[] blockAnswers = Array.Empty<bool>();
        
        public void IncreaseAnswerCount()
        {
            answerCount++;
            Array.Resize(ref blockAnswers, answerCount);
        }
        
        public void DecreaseAnswerCount()
        {
            if (answerCount > 0)
            {
                answerCount--;
                Array.Resize(ref blockAnswers, answerCount);
            }
        }

        private void OnValidate()
        {
            if (blockAnswers.Length != answerCount)
            {
                Array.Resize(ref blockAnswers, answerCount);
            }
        }
    }
}