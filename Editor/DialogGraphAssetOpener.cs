using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor
{
    public class DialogGraphAssetOpener : AssetModificationProcessor
    {
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            // Try to get the asset
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is DialogGraphEditor dialogGraph)
            {
                var window = EditorWindow.GetWindow<DialogGraphEditorWindow>("Dialog Graph");
                window.LoadGraph(dialogGraph);
                return true; // Mark as handled
            }

            return false; // Not handled
        }
    }
}
