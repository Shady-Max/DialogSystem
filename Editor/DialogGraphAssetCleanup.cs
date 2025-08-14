using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;


namespace ShadyMax.DialogSystem.Editor
{
    // Phase 1: Remember which table to delete when a graph is removed
    internal class DialogGraphDeletionTracker : AssetModificationProcessor
    {
        private static readonly Dictionary<string, string> Pending = new();

        static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            // Load only your graph assets; replace "YourGraphTypeName" with your ScriptableObject type
            var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (obj == null)
                return AssetDeleteResult.DidNotDelete;

            var tableName = GetLocalizationTableName(obj);
            if (!string.IsNullOrEmpty(tableName))
            {
                Pending[assetPath] = tableName;
            }

            return AssetDeleteResult.DidNotDelete;
        }

        internal static bool TryDequeue(string assetPath, out string tableName)
        {
            if (Pending.TryGetValue(assetPath, out tableName))
            {
                Pending.Remove(assetPath);
                return true;
            }
            tableName = null;
            return false;
        }

        // Adapt this: read the string field/property on your asset that stores the table name
        private static string GetLocalizationTableName(ScriptableObject asset)
        {
            var field = asset.GetType().GetField("localizationTable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(asset) as string;
        }
    }

    // Phase 2: Perform deletion after Unity finishes the delete operation
    internal class DialogGraphLocalizationPostDelete : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets,
                                           string[] deletedAssets,
                                           string[] movedAssets,
                                           string[] movedFromAssetPaths)
        {
            foreach (var deleted in deletedAssets)
            {
                if (!DialogGraphDeletionTracker.TryDequeue(deleted, out var tableName) || string.IsNullOrEmpty(tableName))
                    continue;

                // If tables can be shared across graphs, bail out if still referenced elsewhere
                if (IsTableReferencedByOtherGraphs(tableName))
                    continue;

                RemoveCollectionByName(tableName);
            }
        }

        private static bool IsTableReferencedByOtherGraphs(string tableName)
        {
            // Replace type filter with your graph type if you have an asmdef/type
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (obj == null) continue;

                var field = obj.GetType().GetField("localizationTable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null) continue;

                var value = field.GetValue(obj) as string;
                if (!string.IsNullOrEmpty(value) && string.Equals(value, tableName, StringComparison.Ordinal))
                {
                    return true; // still in use
                }
            }
            return false;
        }

        private static void RemoveCollectionByName(string tableName)
        {
            // Remove string table collection
            var stringCollection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (stringCollection != null)
            {
                if (TryRemoveCollectionViaReflection(stringCollection))
                {
                    // Also remove corresponding audio collection
                    RemoveAudioCollectionByName(tableName);
                    return;
                }
                RemoveCollectionManually(stringCollection);
            }

            // Remove corresponding audio table collection
            RemoveAudioCollectionByName(tableName);
        }
        
        private static void RemoveAudioCollectionByName(string tableName)
        {
            string audioTableName = tableName + "_Audio";
            var audioCollection = LocalizationEditorSettings.GetAssetTableCollection(audioTableName);
            if (audioCollection != null)
            {
                if (!TryRemoveAudioCollectionViaReflection(audioCollection))
                {
                    RemoveAudioCollectionManually(audioCollection);
                }
            }
        }
        
        private static bool TryRemoveAudioCollectionViaReflection(AssetTableCollection collection)
        {
            try
            {
                var mi = typeof(LocalizationEditorSettings).GetMethod(
                    "RemoveCollection",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(AssetTableCollection) },
                    modifiers: null);
                if (mi != null)
                {
                    mi.Invoke(null, new object[] { collection });
                    AssetDatabase.SaveAssets();
                    return true;
                }
            }
            catch
            {
                // ignore and fallback
            }
            return false;
        }

        private static bool TryRemoveCollectionViaReflection(StringTableCollection collection)
        {
            try
            {
                var mi = typeof(LocalizationEditorSettings).GetMethod(
                    "RemoveCollection",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(StringTableCollection) },
                    modifiers: null);
                if (mi != null)
                {
                    mi.Invoke(null, new object[] { collection });
                    AssetDatabase.SaveAssets();
                    return true;
                }
            }
            catch
            {
                // ignore and fallback
            }
            return false;
        }

        private static StringTableCollection FindCollectionByName(string tableName)
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(StringTableCollection)}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var c = AssetDatabase.LoadAssetAtPath<StringTableCollection>(path);
                if (c != null && string.Equals(c.TableCollectionName, tableName, StringComparison.Ordinal))
                    return c;
            }
            return null;
        }

        public static void RemoveCollectionManually(StringTableCollection collection)
        {
            var toDelete = new List<string>();

            // Collect per-locale table asset paths
            foreach (var table in collection.StringTables.Where(t => t != null))
            {
                var path = AssetDatabase.GetAssetPath(table);
                if (!string.IsNullOrEmpty(path))
                    toDelete.Add(path);
            }

            // Shared data
            var sharedPath = AssetDatabase.GetAssetPath(collection.SharedData);
            if (!string.IsNullOrEmpty(sharedPath))
                toDelete.Add(sharedPath);

            // Collection asset itself
            var collectionPath = AssetDatabase.GetAssetPath(collection);
            if (!string.IsNullOrEmpty(collectionPath))
                toDelete.Add(collectionPath);

            // Delete assets; check result to catch failures (e.g., read-only/VCS)
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var path in toDelete.Distinct())
                {
                    if (string.IsNullOrEmpty(path))
                        continue;

                    var ok = AssetDatabase.DeleteAsset(path);
                    if (!ok)
                    {
                        // Falls back to force import + retry once, then log
                        AssetDatabase.ImportAsset(path);
                        ok = AssetDatabase.DeleteAsset(path);
                        if (!ok)
                            Debug.LogWarning($"Localization cleanup: could not delete asset at '{path}'. It may be read-only or locked by VCS.");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
        
        public static void RemoveAudioCollectionManually(AssetTableCollection collection)
        {
            var toDelete = new List<string>();

            // Collect per-locale table asset paths
            foreach (var tableRef in collection.Tables.Where(t => t.asset != null))
            {
                var table = tableRef.asset;
                var path = AssetDatabase.GetAssetPath(table);
                if (!string.IsNullOrEmpty(path))
                    toDelete.Add(path);
            }

            // Shared data
            var sharedPath = AssetDatabase.GetAssetPath(collection.SharedData);
            if (!string.IsNullOrEmpty(sharedPath))
                toDelete.Add(sharedPath);

            // Collection asset itself
            var collectionPath = AssetDatabase.GetAssetPath(collection);
            if (!string.IsNullOrEmpty(collectionPath))
                toDelete.Add(collectionPath);

            // Delete assets
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var path in toDelete.Distinct())
                {
                    if (string.IsNullOrEmpty(path)) continue;

                    var ok = AssetDatabase.DeleteAsset(path);
                    if (!ok)
                    {
                        AssetDatabase.ImportAsset(path);
                        ok = AssetDatabase.DeleteAsset(path);
                        if (!ok)
                            Debug.LogWarning($"Audio localization cleanup: could not delete asset at '{path}'.");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
    }

    // Optional utility: run manually to clean up any orphans left behind
    internal static class LocalizationOrphanCleaner
    {
        [MenuItem("Tools/Localization/Clean Orphaned String Table Collections")]
        private static void CleanOrphans()
        {
            var collections = AssetDatabase.FindAssets($"t:{nameof(StringTableCollection)}")
                .Select(g => AssetDatabase.LoadAssetAtPath<StringTableCollection>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(c => c != null)
                .ToList();

            // Build set of table names referenced by any existing graph assets
            var referenced = new HashSet<string>(StringComparer.Ordinal);
            var allObjs = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (var guid in allObjs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (obj == null) continue;

                var field = obj.GetType().GetField("localizationTable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var name = field?.GetValue(obj) as string;
                if (!string.IsNullOrEmpty(name))
                    referenced.Add(name);
            }

            foreach (var c in collections)
            {
                if (!referenced.Contains(c.TableCollectionName))
                    DialogGraphLocalizationPostDelete.RemoveCollectionManually(c);
            }

            Debug.Log("Localization cleanup complete.");
        }
        
        [MenuItem("Tools/Localization/Clean Orphaned Audio Table Collections")]
        private static void CleanAudioOrphans()
        {
            var audioCollections = AssetDatabase.FindAssets($"t:{nameof(AssetTableCollection)}")
                .Select(g => AssetDatabase.LoadAssetAtPath<AssetTableCollection>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(c => c != null)
                .ToList();

            var referenced = new HashSet<string>(StringComparer.Ordinal);
            var allObjs = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (var guid in allObjs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (obj == null) continue;

                var field = obj.GetType().GetField("localizationTable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var name = field?.GetValue(obj) as string;
                if (!string.IsNullOrEmpty(name))
                {
                    referenced.Add(name);
                    referenced.Add(name + "_Audio");
                }
            }

            foreach (var c in audioCollections)
            {
                if (!referenced.Contains(c.TableCollectionName))
                    DialogGraphLocalizationPostDelete.RemoveAudioCollectionManually(c);
            }

            Debug.Log("Audio localization cleanup complete.");
        }
    }

}