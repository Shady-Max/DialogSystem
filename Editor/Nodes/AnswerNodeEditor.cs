namespace ShadyMax.DialogSystem.Editor.Nodes
{
    public class AnswerNodeEditor : BaseNodeEditor
    {
        public int answerCount = 0;
        
        public void IncreaseAnswerCount()
        {
            answerCount++;
        }
        
        public void DecreaseAnswerCount()
        {
            if (answerCount > 0)
            {
                answerCount--;
            }
        }
    }
}