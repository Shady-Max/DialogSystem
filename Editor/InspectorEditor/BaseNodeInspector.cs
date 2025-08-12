using System;
using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor.InspectorEditor
{
    [CustomEditor(typeof(BaseNodeEditor))]
    public class BaseNodeInspector<T> : UnityEditor.Editor where T : BaseNodeEditor
    {
        protected T _target;

        protected void OnEnable()
        {
            _target = target as T;
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
                EditorGUILayout.Vector2Field("Position", _target.Position);
                EditorGUILayout.TextField("GUID", _target.Guid);
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}