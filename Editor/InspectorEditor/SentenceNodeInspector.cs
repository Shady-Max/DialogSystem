using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace ShadyMax.DialogSystem.Editor.InspectorEditor
{
    [CustomEditor(typeof(SentenceNodeEditor))]
    public class SentenceNodeInspector : BaseNodeInspector<SentenceNodeEditor>
    {
        private LocalizationTableCollection _tableCollection;
        private List<Locale> _availableLocales;
        private Dictionary<Locale, string> _localizedValues = new Dictionary<Locale, string>();
        private Vector2 _scrollPosition;
        private bool _needsSave = false;

        protected new void OnEnable()
        {
            base.OnEnable();
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            RefreshLocalizationData();
        }
        
        private void RefreshLocalizationData()
        {
            if (string.IsNullOrEmpty(_target.tableReference))
            {
                Debug.LogError("Table reference is empty!");
                return;
            }

            // Get the table collection
            _tableCollection = LocalizationEditorSettings.GetStringTableCollection(_target.tableReference);
            
            if (_tableCollection == null)
            {
                Debug.LogError("Could not find table collection!");
                return;
            }


            // Get all available locales
            _availableLocales = LocalizationEditorSettings.GetLocales().ToList();
            
            // Load current values for each locale
            _localizedValues.Clear();
            foreach (var locale in _availableLocales)
            {
                var table = _tableCollection.GetTable(locale.Identifier) as StringTable;
                if (table != null)
                {
                    _localizedValues[locale] = table.GetEntry(_target.Guid)?.Value ?? string.Empty;
                }
            }
        }
        
        private void UpdateTranslation(Locale locale, string key, string newValue
        )
        {
            if (_tableCollection == null || string.IsNullOrEmpty(key))
            {
                Debug.LogError("Table collection is null!");
                return;
            }

            StringTable table = _tableCollection.GetTable(locale.Identifier) as StringTable;
            if (table == null)
            {
                table = _tableCollection.AddNewTable(locale.Identifier) as StringTable;
                if (table == null)
                {
                    Debug.LogError($"Failed to create table for locale {locale.Identifier}");
                    return;
                }
            }
            
            Undo.RecordObject(table, "Update Translation");
            Undo.RecordObject(table.SharedData, "Update Shared Data");
            
            // Get or create shared table entry
            SharedTableData.SharedTableEntry sharedEntry;
            if (!table.SharedData.Contains(key))
            {
                sharedEntry = table.SharedData.AddKey(key);
            }
            else
            {
                sharedEntry = table.SharedData.GetEntry(key);
            }

            if (sharedEntry != null)
            {

                // Update or create the localized value
                var entry = table.GetEntry(sharedEntry.Id);
                if (entry == null)
                {
                    entry = table.AddEntry(sharedEntry.Id, newValue);
                }
                else
                {
                    entry.Value = newValue;
                }




                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
                EditorUtility.SetDirty(_tableCollection);

                _needsSave = true;
            }
        }



        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Node Data", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                string newAuthor = EditorGUILayout.TextField("Author Name", _target.author);
                if (newAuthor != _target.author)
                {
                    Undo.RecordObject(_target, "Change Author");
                    _target.author = newAuthor;
                    EditorUtility.SetDirty(_target);
                }
            }

            // Basic Info Section
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Localization", EditorStyles.boldLabel);
            
            if (string.IsNullOrEmpty(_target.tableReference))
            {
                EditorGUILayout.HelpBox("No localization table assigned. Please open this node in a Dialog Graph.", 
                    MessageType.Info);
            } else if (_tableCollection == null)
            {
                EditorGUILayout.HelpBox($"Table '{_target.tableReference}' not found.", MessageType.Error);
            }
            else
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true), GUILayout.MinHeight(20), GUILayout.MaxHeight(200));

                    foreach (var locale in _availableLocales)
                    {
                        string currentValue = _localizedValues.ContainsKey(locale) ? _localizedValues[locale] : string.Empty;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(locale.name, GUILayout.Width(100));
                        
                        EditorGUI.BeginChangeCheck();
                        string newValue = EditorGUILayout.TextField(currentValue);
                        if (EditorGUI.EndChangeCheck())
                        {
                            UpdateTranslation(locale, _target.Guid, newValue);
                            _localizedValues[locale] = newValue;
                        }
                        
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(5);
                    }

                    EditorGUILayout.EndScrollView();

                }
            }

            // Add any additional custom fields here

            /*if (_needsSave)
            {
                // Delay save until the end of the frame
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.SaveAssets();
                    _needsSave = false;
                };
            }*/

            
            serializedObject.ApplyModifiedProperties(); 
            _target.OnDataChanged?.Invoke();
        }
        
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            if (_needsSave)
            {
                AssetDatabase.SaveAssets();
                _needsSave = false;
            }
        }

        private void OnUndoRedoPerformed()
        {
            RefreshLocalizationData();
        }

    }
}