using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor.InspectorEditor
{
    [CustomEditor(typeof(AnswerNodeEditor))]
    public class AnswerNodeInspector : BaseNodeInspector<AnswerNodeEditor>
    {
        private bool _changeQueued;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            bool baseChanged = EditorGUI.EndChangeCheck();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Node Data", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.BeginDisabledGroup(true); // Make these read-only
                EditorGUILayout.IntField("Answer Count", _target.answerCount);
                EditorGUI.EndDisabledGroup();
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Node Actions", EditorStyles.boldLabel);
            
            bool changed = baseChanged;
            using (new EditorGUI.IndentLevelScope())
            {
                if (GUILayout.Button("Add Answer"))
                {
                    Undo.RecordObject(_target, "Add Answer");
                    _target.IncreaseAnswerCount();
                    EditorUtility.SetDirty(_target);
                    changed = true;
                }
                            
                if (GUILayout.Button("Remove Answer"))
                {
                    Undo.RecordObject(_target, "Remove Answer");
                    _target.DecreaseAnswerCount();
                    EditorUtility.SetDirty(_target);
                    changed = true;
                }
            }
            
            changed |= serializedObject.ApplyModifiedProperties();
            
            if (changed)
                QueueOnDataChanged();

        }
        
        private void QueueOnDataChanged()
        {
            if (_changeQueued) return;
            _changeQueued = true;

            var node = _target; // capture
            EditorApplication.delayCall += () =>
            {
                _changeQueued = false;
                if (node != null)
                    node.OnDataChanged?.Invoke();
            };
        }

    }
}