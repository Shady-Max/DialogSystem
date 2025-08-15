using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor.InspectorEditor
{
    [CustomEditor(typeof(AnswerNodeEditor))]
    public class AnswerNodeInspector : BaseNodeChildInspector<AnswerNodeEditor>
    {
        private bool _changeQueued;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Node Data", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.BeginDisabledGroup(true); // Make these read-only
                EditorGUILayout.IntField("Answer Count", _target.answerCount);
                EditorGUI.EndDisabledGroup();
            }
            
            bool changed = false;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Block Answers", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                for (int i = 0; i < _target.answerCount; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    bool newBlockAnswer = EditorGUILayout.Toggle($"Answer {i+1}", _target.blockAnswers[i]);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_target, $"Toggle Block Answer {i + 1}");
                        _target.blockAnswers[i] = newBlockAnswer;
                        changed = true;
                        EditorUtility.SetDirty(_target);
                    }
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Node Actions", EditorStyles.boldLabel);
            
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