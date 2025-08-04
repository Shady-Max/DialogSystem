using System.Collections.Generic;
using System.Linq;
using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace ShadyMax.DialogSystem.Editor.InspectorEditor
{
    [CustomEditor(typeof(SentenceNodeEditor))]
    public class SentenceNodeInspector : UnityEditor.Editor
    {
        private SentenceNodeEditor _target;
        private LocalizationTableCollection _tableCollection;
        private List<Locale> _availableLocales;
        private Dictionary<Locale, string> _localizedValues = new Dictionary<Locale, string>();
        private Vector2 _scrollPosition;
        private bool _needsSave = false;

        private void OnEnable()
        {
            _target = (SentenceNodeEditor)target;
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
            if (_tableCollection == null)
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



        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

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
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));

                    foreach (var locale in _availableLocales)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(locale.name, GUILayout.Width(100));
                        string currentValue = _localizedValues.ContainsKey(locale) ? _localizedValues[locale] : string.Empty;
                        string newValue = EditorGUILayout.TextField(currentValue);
                        EditorGUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck())
                        {
                            UpdateTranslation(locale, _target.Guid, newValue);
                            _localizedValues[locale] = newValue;
                        }
                        EditorGUILayout.Space(5);
                    }

                    EditorGUILayout.EndScrollView();

                }
            }


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

            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                _target.OnDataChanged?.Invoke();
            }
        }
        
        private void OnDisable()
        {
            if (_needsSave)
            {
                AssetDatabase.SaveAssets();
                _needsSave = false;
            }
        }


    }
}