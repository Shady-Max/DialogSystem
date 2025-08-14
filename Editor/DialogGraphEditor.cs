using System;
using System.Collections.Generic;
using System.Reflection;
using ShadyMax.DialogSystem.Editor.Nodes;
using ShadyMax.DialogSystem.Editor.Variables;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor
{
    [CreateAssetMenu(fileName = "Editor/DialogGraph", menuName = "Scriptable Objects/DialogGraph")]
    public class DialogGraphEditor : ScriptableObject
    {
        public List<BaseNodeEditor> nodes = new List<BaseNodeEditor>();
        public List<EdgeData> edges = new List<EdgeData>();
        public List<BaseVariable> variables = new List<BaseVariable>();
        public string localizationTable = "";
        public string AudioTableReference => localizationTable + "_Audio";
        
        private void OnEnable()
        {
            if (string.IsNullOrEmpty(localizationTable))
            {
                // Generate a unique table name based on the asset name
                localizationTable = Guid.NewGuid().ToString();

                // Check if the string table already exists
                var stringTableCollection = LocalizationEditorSettings.GetStringTableCollection(localizationTable);

                if (stringTableCollection == null)
                {
                    // Create new string table collection if it doesn't exist
                    stringTableCollection = LocalizationEditorSettings.CreateStringTableCollection(
                        localizationTable,
                        "Assets/Localization/Tables"
                    );
                }
                
                // Check if the audio table already exists
                var audioTableCollection = LocalizationEditorSettings.GetAssetTableCollection(AudioTableReference);

                if (audioTableCollection == null)
                {
                    // Create new audio table collection if it doesn't exist
                    audioTableCollection = LocalizationEditorSettings.CreateAssetTableCollection(
                        AudioTableReference,
                        "Assets/Localization/Tables"
                    );
                }
                
                var locales = LocalizationEditorSettings.GetLocales();
                foreach (var locale in locales)
                {
                    // Create string table for this locale if it doesn't exist
                    if (stringTableCollection.GetTable(locale.Identifier) == null)
                    {
                        stringTableCollection.AddNewTable(locale.Identifier);
                    }

                    // Create asset table for this locale if it doesn't exist
                    if (audioTableCollection.GetTable(locale.Identifier) == null)
                    {
                        audioTableCollection.AddNewTable(locale.Identifier);
                    }
                }
                
                EditorUtility.SetDirty(stringTableCollection);
                EditorUtility.SetDirty(audioTableCollection);
                AssetDatabase.SaveAssets();
            }
            
            nodes ??= new List<BaseNodeEditor>();
            edges ??= new List<EdgeData>();
            variables ??= new List<BaseVariable>();

            var removed = RemoveInvalidEdges(autoSave: true);
            if (removed > 0)
            {
                // Also clean any stray sub-assets that might have been left behind
                CleanupDeletedNodes();
            }
        }
        
        private void OnDisable()
        {
            // Clean invalid edges when this asset is unloaded/disabled in the editor
            RemoveInvalidEdges(autoSave: true);
        }

        public int RemoveInvalidEdges(bool autoSave = false)
        {
            if (edges == null || edges.Count == 0)
                return 0;

            // Register undo so the operation is reversible in the editor
            Undo.RegisterCompleteObjectUndo(this, "Remove Invalid Edges");

            int before = edges.Count;
            edges.RemoveAll(IsEdgeInvalid);

            int removed = before - edges.Count;
            if (removed > 0 && autoSave)
            {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }

            return removed;
        }

        private bool IsEdgeInvalid(EdgeData edge)
        {
            if (edge == null)
                return true;

            // Reflect over EdgeData to find any BaseNodeEditor references.
            // If any referenced node is null or not part of this graph, the edge is invalid.
            var fields = edge.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            foreach (var field in fields)
            {
                if (typeof(BaseNodeEditor).IsAssignableFrom(field.FieldType))
                {
                    var node = field.GetValue(edge) as BaseNodeEditor;

                    if (node == null)
                        return true;

                    if (nodes == null || !nodes.Contains(node))
                        return true;
                }
            }

            // If no node references were found on the edge, we cannot validate it here.
            // Keep it to avoid false positives (it may be ID-based). If you use IDs,
            // consider extending this method to validate those IDs against your node list.
            return false;
        }

        
        public void CleanupDeletedNodes()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
            bool needsSave = false;
        
            foreach (var asset in assets)
            {
                if (asset is BaseNodeEditor node && 
                    node.hideFlags == HideFlags.HideInHierarchy && 
                    !nodes.Contains(node))
                {
                    Undo.DestroyObjectImmediate(node);
                    needsSave = true;
                }
            }

            if (needsSave)
            {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }


    }
}
