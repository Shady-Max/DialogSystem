using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace ShadyMax.DialogSystem.Editor.Nodes
{
    public class SentenceNodeEditor : BaseNodeEditor
    {
        public string author;
        
        private void OnEnable()
        {
            // Add this to your existing OnEnable method
            RestoreLocalizationIfNeeded();
            Undo.undoRedoPerformed += OnUndoRedo;
        }
        
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnDestroy()
        {
            if (!Undo.isProcessing)
            {
                RemoveLocalizationEntries();
            }
        }
        
        private void OnUndoRedo()
        {
            RestoreLocalizationIfNeeded();
        }
        
        private void RemoveLocalizationEntries()
        {
            if (string.IsNullOrEmpty(tableReference))
                return;

            var tableCollection = LocalizationEditorSettings.GetStringTableCollection(tableReference);
            if (tableCollection == null)
                return;

            // Store all the data we need to restore in case of undo
            var undoData = new System.Collections.Generic.Dictionary<StringTable, string>();
        
            // Register undo for the table collection and all its tables
            Undo.RegisterCompleteObjectUndo(tableCollection, "Remove Localization Entries");
            Undo.RegisterCompleteObjectUndo(tableCollection.SharedData, "Remove Localization Entries");

            foreach (var locale in LocalizationEditorSettings.GetLocales())
            {
                var table = tableCollection.GetTable(locale.Identifier) as StringTable;
                if (table == null)
                    continue;

                // Register undo for the table before we modify it
                Undo.RegisterCompleteObjectUndo(table, "Remove Localization Entries");

                var sharedEntry = table.SharedData.GetEntry(Guid);
                if (sharedEntry != null)
                {
                    // Store the old value
                    var oldEntry = table.GetEntry(sharedEntry.Id);
                    if (oldEntry != null)
                    {
                        undoData[table] = oldEntry.Value;
                    }

                    // Remove from this specific table
                    table.RemoveEntry(sharedEntry.Id);
                    EditorUtility.SetDirty(table);
                }
            }

            // Remove from shared data
            if (tableCollection.SharedData.Contains(Guid))
            {
                tableCollection.SharedData.RemoveKey(Guid);
                EditorUtility.SetDirty(tableCollection.SharedData);
            }

            EditorUtility.SetDirty(tableCollection);
            AssetDatabase.SaveAssets();

            // Store the undo data in EditorPrefs for restoration
            if (undoData.Count > 0)
            {
                StoreUndoData(undoData);
            }

        }
        
        private void StoreUndoData(System.Collections.Generic.Dictionary<StringTable, string> undoData)
        {
            var undoGroup = Undo.GetCurrentGroup();
            foreach (var kvp in undoData)
            {
                var key = $"LocalizationUndo_{undoGroup}_{kvp.Key.GetInstanceID()}";
                EditorPrefs.SetString(key, kvp.Value);
            }
        }

        private void RestoreLocalizationIfNeeded()
        {
            if (Undo.isProcessing)
            {
                var undoGroup = Undo.GetCurrentGroup();
                var tableCollection = LocalizationEditorSettings.GetStringTableCollection(tableReference);
                if (tableCollection == null) return;

                foreach (var locale in LocalizationEditorSettings.GetLocales())
                {
                    var table = tableCollection.GetTable(locale.Identifier) as StringTable;
                    if (table == null) continue;

                    var key = $"LocalizationUndo_{undoGroup}_{table.GetInstanceID()}";
                    var savedValue = EditorPrefs.GetString(key, null);
                    if (!string.IsNullOrEmpty(savedValue))
                    {
                        // Restore the entry
                        if (!table.SharedData.Contains(Guid))
                        {
                            table.SharedData.AddKey(Guid);
                        }
                        var sharedEntry = table.SharedData.GetEntry(Guid);
                        if (sharedEntry != null)
                        {
                            table.AddEntry(sharedEntry.Id, savedValue);
                            EditorUtility.SetDirty(table);
                        }
                    
                        // Clean up the stored data
                        EditorPrefs.DeleteKey(key);
                    }
                }
            }
        }

    }
    

}