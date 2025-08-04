using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor.ViewNodes
{
    public class SentenceNodeView : BaseNodeView<SentenceNodeEditor>
    {
        public Port inputPort;
        public Port outputPort;
        
        private TextField _authorField;

        public override void Initialize(BaseNodeEditor node, DialogGraphView dialogGraphView)
        {
            mainContainer.style.minWidth = 220;
            base.Initialize(node, dialogGraphView);
        }

        protected override string GetTitle() => "Sentence Node";

        protected override void CreateInputPorts()
        {
            inputContainer.Clear();
            
            inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(BaseNodeEditor));
            inputPort.portName = "In";
            inputPort.name = "In";
            inputContainer.Add(inputPort);
        }

        protected override void CreateOutputPorts()
        {
            outputContainer.Clear();
            
            outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BaseNodeEditor));
            outputPort.portName = "Out";
            outputPort.name = "Out";
            outputContainer.Add(outputPort);
        }

        protected override void CreateContent()
        {
            extensionContainer.Clear();
            
            _authorField = new TextField("Author Name") { value = node.author };
            _authorField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(node, "Change Author");
                node.author = evt.newValue;
                EditorUtility.SetDirty(node);
                GraphView.GraphChanged?.Invoke();
            });
            _authorField.labelElement.style.minWidth = 90;
            extensionContainer.Add(_authorField);
        }

        protected override void RefreshUI()
        {
            _authorField.value = node.author;
        }
    }
}