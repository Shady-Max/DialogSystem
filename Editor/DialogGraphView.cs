using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.SEIDInfo;
using ShadyMax.DialogSystem.Editor.Nodes;
using ShadyMax.DialogSystem.Editor.Variables;
using ShadyMax.DialogSystem.Editor.ViewNodes;
using ShadyMax.DialogSystem.Runtime.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ShadyMax.DialogSystem.Editor
{
    public class DialogGraphView : GraphView
    {
        public Action GraphChanged;
        public DialogGraphEditor DialogReference => _dialogReference;

        private DialogGraphEditor _dialogReference;
        private NodeSearchWindow _nodeSearchWindow;
        private VariableBlackboard _blackboard;
        
        private Vector2 _lastRightClickPosition;
        
        private static readonly Dictionary<Type, Type> NodeToViewMapping = new()
        {
            {typeof(BeginNodeEditor), typeof(BeginNodeView)},
            {typeof(SentenceNodeEditor), typeof(SentenceNodeView)},
            {typeof(AnswerNodeEditor), typeof(AnswerNodeView)},
            {typeof(IfNodeEditor), typeof(IfNodeVIew)},
            {typeof(NotNodeEditor), typeof(NotNodeView)},
            {typeof(AndNodeEditor), typeof(AndNodeView)},
            {typeof(OrNodeEditor), typeof(OrNodeView)},
            {typeof(VariableGetNodeEditor), typeof(VariableGetNodeView)},
            {typeof(VariableSetNodeEditor), typeof(VariableSetNodeView)}
        };
        

        public DialogGraphView()
        {
            AddManipulators();
            AddGridBackground();
            AddStyles();
            AddBlackboard();
            AddSearchWindow();
            RegisterCallbacks();
            Undo.undoRedoPerformed += OnUndoRedo;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterDomainReload;
        }


        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContextualMenuManipulator(evt => nodeCreationRequest?.Invoke(new NodeCreationContext
            {
                screenMousePosition = evt.mousePosition
            })));
        }

        private void AddGridBackground()
        {
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        }
        
        private void AddStyles()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/Styles/DialogGraph.uss") ?? AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/DialogSystem/Editor/Styles/DialogGraph.uss");
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }
        }
        
        private void RegisterCallbacks()
        {
            RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 1) // Right mouse button 
                {
                    // Convert screen to local position
                    _lastRightClickPosition = contentViewContainer.WorldToLocal(evt.position);
                }
            });
            
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
                {
                    DeleteElements(selection.OfType<GraphElement>());
                    evt.StopPropagation();
                }
            });
            
            RegisterCallback<DragUpdatedEvent>(evt =>
            {
                if (DragAndDrop.GetGenericData("BlackboardField") != null)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.StopPropagation();
                }
            });
            
            RegisterCallback<DragPerformEvent>(evt =>
            {
                var variable = DragAndDrop.GetGenericData("BlackboardField") as BaseVariable;
                if (variable != null)
                {
                    DragAndDrop.AcceptDrag();

                    // Convert mouse position to graph space
                    var localMousePos = contentViewContainer.WorldToLocal(evt.mousePosition);

                    // Create your node here
                    CreateVariableGetNode(variable.guid, localMousePos);
                }
            });
            
            graphViewChanged += OnGraphViewChanged;
        }
        
        public void CreateVariableGetNode(string variableGuid, Vector2 position)
        {
            var type = typeof(VariableGetNodeEditor);
            VariableGetNodeEditor newGetNode = ScriptableObject.CreateInstance<VariableGetNodeEditor>();
            if (newGetNode == null) return;

            var guid = Guid.NewGuid().ToString();
            newGetNode.name = $"{type.Name}_{guid}";
            newGetNode.variableGuid = variableGuid;
            newGetNode.Guid = guid;
            newGetNode.tableReference = _dialogReference.localizationTable;
            newGetNode.Position = position;
            
            Undo.RegisterCompleteObjectUndo(new Object[] { newGetNode, _dialogReference }, "Create Node");

            AssetDatabase.AddObjectToAsset(newGetNode, _dialogReference);
            AssetDatabase.SaveAssets();
            
            _dialogReference.nodes.Add(newGetNode);

            CreateNodeView(newGetNode);

            EditorUtility.SetDirty(_dialogReference);
            GraphChanged?.Invoke();
        }
        
        private void OnAfterDomainReload()
        {
            if (_dialogReference != null)
            {
                // Force deserialization after domain reload
                foreach (var variable in _dialogReference.variables)
                {
                    variable?.OnAfterDeserialize();
                }
        
                // Refresh the blackboard
                if (_blackboard is VariableBlackboard variableBlackboard)
                {
                    variableBlackboard.LoadVariables(_dialogReference.variables);
                }
            }
        }
        
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (_dialogReference == null)
            {
                Debug.LogWarning("_dialogReference is null");
                return graphViewChange;
            }

            bool hasChanges = false;
            
            if (graphViewChange.edgesToCreate != null)
            {
                    
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    var outputNode = edge.output.node as Node;
                    var inputNode = edge.input.node as Node;

                    if (outputNode?.userData is BaseNodeEditor fromNode &&
                        inputNode?.userData is BaseNodeEditor toNode)
                    {
                        Undo.RegisterCompleteObjectUndo(new Object[] { _dialogReference, fromNode }
                            , "Create Connection");
                        
                        // Create new edge data
                        var edgeData = new EdgeData
                        {
                            fromNode = fromNode.Guid,
                            toNode = toNode.Guid,
                            fromPort = edge.output.name,
                            toPort = edge.input.name
                        };
                
                        // Add the edge to the dialog reference's edge list
                        _dialogReference.edges.Add(edgeData);
                        EditorUtility.SetDirty(_dialogReference);
                        hasChanges = true;
                    }
                }
            }
                
            if (graphViewChange.elementsToRemove != null)
            {
                DeleteElements(graphViewChange.elementsToRemove);
                hasChanges = true;
            }
            if (hasChanges)
            {
                GraphChanged?.Invoke();
                AssetDatabase.SaveAssets();
            }
            return graphViewChange;
        }

        private void AddSearchWindow()
        {
            _nodeSearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _nodeSearchWindow.Initialize(this);
            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _nodeSearchWindow);
            };
        }

        private void AddBlackboard()
        {
            _blackboard = new VariableBlackboard(this);
            Add(_blackboard);
        }
        
        public void SaveVariables(List<BaseVariable> variables)
        {
            if (_dialogReference == null) return;

            Undo.RegisterCompleteObjectUndo(_dialogReference, "Save Variables");
            _dialogReference.variables = variables;
            EditorUtility.SetDirty(_dialogReference);
            GraphChanged?.Invoke();
            AssetDatabase.SaveAssets();
        }

        public void OnVariableAdded(BaseVariable variable)
        {
            if (_dialogReference == null) return;

            Undo.RegisterCompleteObjectUndo(_dialogReference, "Add Variable");
            _dialogReference.variables.Add(variable);
            _blackboard.AddVariableField(variable);
            EditorUtility.SetDirty(_dialogReference);
            GraphChanged?.Invoke();
            AssetDatabase.SaveAssets();
        }

        public void OnVariableRemoved(string variableGuid)
        {
            if (_dialogReference == null) return;

            Undo.RegisterCompleteObjectUndo(_dialogReference, "Remove Variable");
            _dialogReference.variables.RemoveAll(v => v.guid == variableGuid);
            EditorUtility.SetDirty(_dialogReference);
            GraphChanged?.Invoke();
            AssetDatabase.SaveAssets();
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            var allPorts = ports.ToList();

            allPorts.ForEach((port) =>
            {
                if (startPort == port)
                    return;

                if (startPort.node == port.node)
                    return;

                if (startPort.direction == port.direction)
                    return;

                bool isCompatible = true;
                
                if (startPort is CustomPort customStartPort)
                {
                    isCompatible = customStartPort.IsConnectionAllowed(port);
                }

                if (port is CustomPort customEndPort && isCompatible)
                {
                    isCompatible = customEndPort.IsConnectionAllowed(startPort);
                }

                if (isCompatible)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        public Vector2 GetLastMousePosition()
        {
            return _lastRightClickPosition;
        }

        public void CreateNode(Type type)
        {
            if (type == typeof(BeginNodeEditor))
            {
                Debug.LogWarning("BeginNode cannot be created manually");
                return;
            }
            
            if (!typeof(BaseNodeEditor).IsAssignableFrom(type))
                return;

            BaseNodeEditor newNode = ScriptableObject.CreateInstance(type) as BaseNodeEditor;
            if (newNode == null) return;

            var guid = Guid.NewGuid().ToString();
            newNode!.name = $"{type.Name}_{guid}";
            newNode.Guid = guid;
            newNode.tableReference = _dialogReference.localizationTable;
            newNode.Position = _lastRightClickPosition;
            
            Undo.RegisterCompleteObjectUndo(new Object[] { newNode, _dialogReference }, "Create Node");

            AssetDatabase.AddObjectToAsset(newNode, _dialogReference);
            AssetDatabase.SaveAssets();
            
            _dialogReference.nodes.Add(newNode);

            CreateNodeView(newNode);

            EditorUtility.SetDirty(_dialogReference);
            GraphChanged?.Invoke();
        }
        
        public Node CreateNodeView(BaseNodeEditor nodeEditor)
        {
            var nodeType = nodeEditor.GetType();
            if (NodeToViewMapping.TryGetValue(nodeType, out Type viewType))
            {
                var view = Activator.CreateInstance(viewType) as Node;
                if (view is INodeView nodeView)
                {
                    nodeView.Initialize(nodeEditor, this);
                    AddElement(view);
                    return view;
                }
            }
            return null;
        }
        
        public new void DeleteElements(IEnumerable<GraphElement> elements)
        {
            if (!elements.Any() || _dialogReference == null) return;
            
            var elementsToDelete = elements.ToList();
            
            // Filter out BeginNode from deletion
            elementsToDelete = elementsToDelete.Where(element =>
            {
                if (element is Node node && node.userData is BeginNodeEditor)
                {
                    Debug.LogWarning("BeginNode cannot be deleted");
                    return false;
                }
                return true;
            }).ToList();

            if (!elementsToDelete.Any()) return;
            
            var objectsToUndo = new List<Object> { _dialogReference };
            
            foreach (var element in elementsToDelete)
            {
                if (element is Node node && node.userData is BaseNodeEditor nodeEditor)
                {
                    objectsToUndo.Add(nodeEditor);
                }
            }

            Undo.RegisterCompleteObjectUndo(objectsToUndo.ToArray(), "Delete Dialog Elements");

            foreach (var element in elementsToDelete)
            {
                if (element is Node node)
                {
                    var connectedEdges = edges.Where(e => e.input.node == node || e.output.node == node).ToList();
                    foreach (var edge in connectedEdges)
                    {
                        DeleteConnection(edge);
                    }

                    
                    if (node.userData is BaseNodeEditor bn && _dialogReference.nodes.Contains(bn))
                    {
                        string guid = bn.Guid;
                        
                        SerializedObject so = new SerializedObject(bn);
                        SerializedProperty guidProp = so.FindProperty("_guid");
                        string storedGuid = guidProp.stringValue;
                        
                        Undo.RecordObject(bn, "Mark Node As Deleted");
                        bn.name = $"DELETED_{storedGuid}";
                        bn.Guid = "";
                        bn.hideFlags = HideFlags.HideInHierarchy;
                        
                        _dialogReference.nodes.Remove(bn);
                        Undo.DestroyObjectImmediate(bn);
                        EditorUtility.SetDirty(_dialogReference);
                    }
                    
                    RemoveElement(node);

                }
                else if (element is Edge edge)
                {
                    DeleteConnection(edge);
                }
            }

            AssetDatabase.SaveAssets();
            GraphChanged?.Invoke();
        }
        
        private void DeleteConnection(Edge edge)
        {
            var outputNode = edge.output.node as Node;
            var inputNode = edge.input.node as Node;

            if (outputNode?.userData is BaseNodeEditor fromNode &&
                inputNode?.userData is BaseNodeEditor toNode)
            {
                Undo.RegisterCompleteObjectUndo(_dialogReference, "Delete Connection");
                
                // Remove the edge from the dialog reference's edge list
                _dialogReference.edges.RemoveAll(e => 
                    e.fromNode == fromNode.Guid && 
                    e.toNode == toNode.Guid &&
                    e.fromPort == edge.output.name &&
                    e.toPort == edge.input.name);
        
                EditorUtility.SetDirty(_dialogReference);
            }
            
            Debug.Log(edge);
            Debug.Log(edge.output is CustomPort);
            Debug.Log(edge.input is CustomPort);
            
            if (edge.output is CustomPort customOutput)
                customOutput.TriggerPortDisconnect(edge);
            if (edge.input is CustomPort customInput)
                customInput.TriggerPortDisconnect(edge);
            
            edge.input.Disconnect(edge);
            edge.output.Disconnect(edge);

            RemoveElement(edge);
        }
        
        public override EventPropagation DeleteSelection()
        {
            DeleteElements(selection.OfType<GraphElement>());
            return EventPropagation.Stop;
        }
        
        public void LoadGraph(DialogGraphEditor graph, bool forceReload = false)
        {
            if (graph == null) return;
            if (_dialogReference == graph && !forceReload) return;
            if (_dialogReference != null)
            {
                _dialogReference.CleanupDeletedNodes();
            }

            ClearGraphView();
            _dialogReference = graph;
            
            // Load variables into blackboard
            if (_blackboard is VariableBlackboard variableBlackboard)
            {
                variableBlackboard.LoadVariables(graph.variables);
            }
            
            Debug.Log($"Loading DialogGraph with {graph.nodes.Count} nodes, {graph.edges.Count} edges, {graph.variables.Count} variables");
            
            var beginNode = graph.nodes.OfType<BeginNodeEditor>().FirstOrDefault();
            if (beginNode == null)
            {
                beginNode = ScriptableObject.CreateInstance<BeginNodeEditor>();
                beginNode.name = "Begin Node";
                beginNode.Guid = System.Guid.NewGuid().ToString();
                beginNode.Position = Vector2.zero;
        
                AssetDatabase.AddObjectToAsset(beginNode, graph);
                graph.nodes.Insert(0, beginNode); // Add at beginning
                EditorUtility.SetDirty(graph);
            }
            
            foreach (var node in graph.nodes)
            {
                if (node == null) continue;
                CreateNodeView(node);
            }

            // Create edges from saved edge data
            foreach (var edgeData in graph.edges)
            {
                var fromNode = graph.nodes.Find(n => n.Guid == edgeData.fromNode);
                var toNode = graph.nodes.Find(n => n.Guid == edgeData.toNode);

                if (fromNode == null || toNode == null) continue;

                // Find the corresponding node views
                var fromNodeView = GetNodeByGuid(edgeData.fromNode);
                var toNodeView = GetNodeByGuid(edgeData.toNode);

                if (fromNodeView == null || toNodeView == null) continue;
                
                Port outputPort = null;
                Port inputPort = null;

                // Find the ports using the stored port names
                outputPort = fromNodeView.Query<Port>().Where(p => p.name == edgeData.fromPort).First();
                inputPort = toNodeView.Query<Port>().Where(p => p.name == edgeData.toPort).First();

                if (outputPort == null || inputPort == null) continue;

                // Create and add the edge
                var edge = outputPort.ConnectTo(inputPort);
                AddElement(edge);
            }
        }
        
        public void UnloadGraph()
        {
            if (_dialogReference != null)
            {
                _dialogReference.CleanupDeletedNodes();
            }
            ClearGraphView();
            _dialogReference = null;
        }


        private new Node GetNodeByGuid(string guid)
        {
            return nodes.ToList()
                .Cast<Node>()
                .FirstOrDefault(x => (x.userData as BaseNodeEditor)?.Guid == guid);
        }

        private void ClearGraphView()
        {
            var elementsToRemove = graphElements.ToList();
            foreach (var element in elementsToRemove)
            {
                RemoveElement(element);
            }
        }

        public void OnDestroy()
        {
            UnloadGraph();
            Undo.undoRedoPerformed -= OnUndoRedo;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterDomainReload;
        }
        
        private void OnUndoRedo()
        {
            // Reload the graph to reflect changes
            if (_dialogReference != null)
            {
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_dialogReference)))
                {
                    if (asset is BaseNodeEditor node && node.hideFlags == HideFlags.HideInHierarchy)
                    {
                        node.hideFlags = HideFlags.None;
                        node.Initialize(); // This will restore the proper name and GUID
                        EditorUtility.SetDirty(node);
                    }
                }

                
                LoadGraph(_dialogReference, true);
                
                foreach (var nodeView in nodes.ToList())
                {
                    if (nodeView is INodeView view && nodeView.userData is BaseNodeEditor nodeEditor)
                    {
                        view.Initialize(nodeEditor, this);
                    }
                }

                GraphChanged?.Invoke();
            }
        }
    }
}
