using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor.ViewNodes
{
    public abstract class BaseNodeView<T> : Node, INodeView where T : BaseNodeEditor 
    {
        public T node;
        protected DialogGraphView GraphView;
        
        public virtual void Initialize(BaseNodeEditor node, DialogGraphView graphView)
        {
            this.node = node as T;
            GraphView = graphView;

            this.userData = node;
            node.OnDataChanged += OnDataChanged;
            title = GetTitle();
            viewDataKey = node.Guid;
            
            CreateInputPorts();
            CreateOutputPorts();
            CreateContent();
            
            SetPosition(new Rect(node.Position, new Vector2(200, 150)));
            RefreshExpandedState();
            RefreshPorts();
        }

        private void OnDataChanged()
        {
            RefreshUI();
        }

        protected abstract string GetTitle();
        protected abstract void CreateInputPorts();
        protected abstract void CreateOutputPorts();
        protected abstract void CreateContent();

        protected abstract void RefreshUI();

        public override void OnSelected()
        {
            base.OnSelected();
            Selection.activeObject = node;
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            if (Selection.activeObject == node)
            {
                Selection.activeObject = null;
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            if (node == null) return;
            Undo.RegisterCompleteObjectUndo(node, "Move Node");
            node.Position = newPos.position;
            EditorUtility.SetDirty(node); // Mark for save
        }

    }
}