using System;
using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor.InspectorEditor
{
    [CustomEditor(typeof(BaseNodeEditor), true)]
    public class BaseNodeInspector : UnityEditor.Editor
    {
        protected BaseNodeEditor _baseNodeTarget;

        protected void OnEnable()
        {
            _baseNodeTarget = (BaseNodeEditor)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Node Properties Section
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Node Properties", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.BeginDisabledGroup(true); // Make these read-only
                EditorGUILayout.Vector2Field("Position", _baseNodeTarget.Position);
                EditorGUILayout.TextField("GUID", _baseNodeTarget.Guid);
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}