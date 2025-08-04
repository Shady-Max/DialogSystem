using System;
using System.Collections.Generic;
using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor.Localization;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor
{
    [CreateAssetMenu(fileName = "Editor/DialogGraph", menuName = "Scriptable Objects/DialogGraph")]
    public class DialogGraphEditor : ScriptableObject
    {
        public List<BaseNodeEditor> nodes = new List<BaseNodeEditor>();
        public List<EdgeData> edges = new List<EdgeData>();
        public string localizationTable = "";
        
        private void OnEnable()
        {
            if (string.IsNullOrEmpty(localizationTable))
            {
                // Generate a unique table name based on the asset name
                localizationTable = Guid.NewGuid().ToString();
                
                // Check if the table already exists
                var tableCollection = LocalizationEditorSettings.GetStringTableCollection(localizationTable);
                
                if (tableCollection == null)
                {
                    // Create new table collection if it doesn't exist
                    tableCollection = LocalizationEditorSettings.CreateStringTableCollection(
                        localizationTable,
                        "Assets/Localization/Tables"
                        );
                }
            }
        }

    }
}
