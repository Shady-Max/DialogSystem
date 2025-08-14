using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor
{
    public class DialogGraphEditorWindow : EditorWindow
    {
        private DialogGraphView _graphView;
        private DialogGraphEditor _currentGraph;
        
        private bool _hasUnsavedChanges;
        
        private const string MenuPath = "Window/Dialog Graph Editor";
        private const string ShowVariablesKey = "DialogGraph_ShowVariables";

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            var window = GetWindow<DialogGraphEditorWindow>("Dialog Graph");
            window.Show();
        }
        
        [Shortcut("Dialog Graph/Save", typeof(DialogGraphEditorWindow), KeyCode.S, ShortcutModifiers.Control)]
        private static void HandleSaveShortcut()
        {
            var window = GetWindow<DialogGraphEditorWindow>();
        
            // Only handle the shortcut if our window is focused
            if (focusedWindow is DialogGraphEditorWindow)
            {
                if (window != null && window._hasUnsavedChanges)
                {
                    window.SaveChanges();
                    Event.current.Use();

                }
            }
        }
        
        private void OnEnable()
        {
            CreateGraphView();
        }
        
        private void OnDisable()
        {
            if (_graphView != null)
            {
                _graphView.GraphChanged -= OnGraphChanged;
                _graphView.UnloadGraph();
            }
        }
        
        private void OnGraphChanged()
        {
            if (!_hasUnsavedChanges)
            {
                _hasUnsavedChanges = true;
                UpdateWindowTitle();
            }
        }
        
        private void UpdateWindowTitle()
        {
            string title = "Dialog Graph";
            if (_currentGraph != null)
            {
                string graphName = _currentGraph.name;
                title = $"{graphName}{(_hasUnsavedChanges ? "*" : "")}";
            }
            titleContent = new GUIContent(title);
        }
        
        private void CreateGraphView()
        {
            var graphViewContainer = new VisualElement();
            graphViewContainer.style.flexGrow = 1;
            graphViewContainer.style.position = Position.Relative;
            graphViewContainer.style.overflow = Overflow.Hidden;
            
            _graphView = new DialogGraphView();
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/Styles/DialogGraph.uss") ?? AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/DialogSystem/Editor/Styles/DialogGraph.uss");
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }
            
            rootVisualElement.Clear();
            var mainContainer = new VisualElement();
            mainContainer.style.flexDirection = FlexDirection.Row;
            mainContainer.style.flexGrow = 1;
            
            _graphView.style.position = Position.Absolute;
            _graphView.style.top = 0;
            _graphView.style.left = 0;
            _graphView.style.right = 0;
            _graphView.style.bottom = 0;
            graphViewContainer.Add(_graphView);
            _graphView.GraphChanged += OnGraphChanged;
            
            mainContainer.Add(graphViewContainer);
            
            rootVisualElement.Add(mainContainer);
            
            // If we had a graph loaded before, reload it
            if (_currentGraph != null)
            {
                LoadGraph(_currentGraph);
            }
        }
        
        private void OnDestroy()
        {
            if (_graphView != null)
            {
                _graphView.OnDestroy();
            }
            if (_hasUnsavedChanges)
            {
                bool save = EditorUtility.DisplayDialog("Unsaved Changes",
                    "There are unsaved changes. Do you want to save them?",
                    "Save", "Discard");
                if (save)
                {
                    SaveChanges();
                }
            }
        }
        
        public override void SaveChanges()
        {
            if (_currentGraph != null)
            {
                EditorUtility.SetDirty(_currentGraph);
                AssetDatabase.SaveAssets();
                _hasUnsavedChanges = false;
                UpdateWindowTitle();
            }
            base.SaveChanges();
        }
        
        public void LoadGraph(DialogGraphEditor graph)
        {
            if (_hasUnsavedChanges && _currentGraph != null)
            {
                if (EditorUtility.DisplayDialog("Unsaved Changes",
                        "There are unsaved changes in the current graph. Do you want to save them?",
                        "Save", "Discard"))
                {
                    SaveChanges();
                }
            }
            
            _currentGraph = graph;

            if (_graphView == null)
            {
                CreateGraphView();
            }

            _graphView!.LoadGraph(graph);
            _hasUnsavedChanges = false;
            UpdateWindowTitle();

            Focus();
        }
    }
}
