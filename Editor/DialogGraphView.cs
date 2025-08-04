using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.SEIDInfo;
using ShadyMax.DialogSystem.Editor.Nodes;
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

        private DialogGraphEditor _dialogReference;
        private NodeSearchWindow _nodeSearchWindow;
        private Blackboard _blackboard;
        
        private Vector2 _lastRightClickPosition;
        
        private static readonly Dictionary<Type, Type> NodeToViewMapping = new()
        {
            {typeof(SentenceNodeEditor), typeof(SentenceNodeView)}
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
            
            graphViewChanged = OnGraphViewChanged;
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
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach((port) =>
            {
                if (startPort == port)
                    return;

                if (startPort.node == port.node)
                    return;

                if (startPort.direction == port.direction)
                    return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        public Vector2 GetLastMousePosition()
        {
            return _lastRightClickPosition;
        }

        public void CreateNode(Type type, Vector2 screenMousePosition)
        {
            if (!typeof(BaseNodeEditor).IsAssignableFrom(type))
                return;

            BaseNodeEditor newNode = ScriptableObject.CreateInstance(type) as BaseNodeEditor;
            if (newNode == null) return;

            var guid = Guid.NewGuid().ToString();
            newNode!.name = $"{type.Name}_{guid}";
            newNode.Guid = guid;
            newNode.tableReference = _dialogReference.localizationTable;

            Debug.Log(newNode != null);
            Debug.Log(_dialogReference != null);
            
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
            
            // Register undo for the entire graph before deletion
            Undo.RegisterCompleteObjectUndo(_dialogReference, "Delete Dialog Elements");


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
                        bn.name = $"DELETED_{guid}";
                        
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
            
            ClearGraphView();
            _dialogReference = graph;
            
            Debug.Log($"Loading DialogGraph with {graph.nodes.Count} nodes, {graph.edges.Count} edges");
            
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

                // Find the ports using the stored port names
                var outputPort = fromNodeView.outputContainer.Q<Port>(edgeData.fromPort) as Port ??
                                 fromNodeView.outputContainer.Q<Port>(name: null)?.ElementAt(0) as Port;
                var inputPort = toNodeView.inputContainer.Q<Port>(edgeData.toPort) as Port ??
                                toNodeView.inputContainer.Q<Port>(name: null)?.ElementAt(0) as Port;

                if (outputPort == null || inputPort == null) continue;

                // Create and add the edge
                var edge = outputPort.ConnectTo(inputPort);
                AddElement(edge);
            }

            /*if (graph != null && graph.startNode == null)
            {
                // Create begin node if it doesn't exist
                BeginNode beginNode = ScriptableObject.CreateInstance<BeginNode>();
                beginNode.guid = System.Guid.NewGuid().ToString();
                beginNode.name = "Begin Node";
    
                AssetDatabase.AddObjectToAsset(beginNode, graph);
                graph.startNode = beginNode;
                graph.nodes.Add(beginNode);
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
            }
            
            var startNode = graph.startNode;
            var beginNodeView = new BeginNodeView(startNode, this);
            beginNodeView.userData = startNode;
            AddElement(beginNodeView);
            nodeViewMap[startNode.guid] = beginNodeView;

            foreach (var node in graph.nodes)
            {
                if (node == null) continue;
                
                Node nodeView = null;
                
                if (node is SentenceNode sentenceNode)
                {
                    nodeView  = CreateSentenceNodeView(sentenceNode);
                }
                else if (node is AnswerNode answerNode)
                {
                    nodeView  = CreateAnswerNodeView(answerNode);
                } else if (node is IfNode ifNode)
                {
                    nodeView  = CreateIfNodeView(ifNode);
                } else if (node is SimpleVariableMathNode simpleVariableMathNode)
                {
                    if (simpleVariableMathNode.mathType == SimpleMathType.Set)
                        nodeView = CreateSetVariableNodeView(simpleVariableMathNode);
                    if (simpleVariableMathNode.mathType == SimpleMathType.Add)
                        nodeView = CreateAddVariableNodeView(simpleVariableMathNode);
                    if (simpleVariableMathNode.mathType == SimpleMathType.Subtract)
                        nodeView = CreateSubtractVariableNodeView(simpleVariableMathNode);
                    if (simpleVariableMathNode.mathType == SimpleMathType.Multiply)
                        nodeView = CreateMultiplyVariableNodeView(simpleVariableMathNode);
                    if (simpleVariableMathNode.mathType == SimpleMathType.Divide)
                        nodeView = CreateDivideVariableNodeView(simpleVariableMathNode);
                }

                if (nodeView  != null)
                {
                    nodeView .userData = node;
                    AddElement(nodeView );
                    if (node is BaseNode baseNode &&
                        !string.IsNullOrEmpty(baseNode.guid))
                        nodeViewMap[baseNode.guid] = nodeView ;
                }
            }
            
            string beginNextGuid = graph.startNode.nextNodeGuid;

            if (!string.IsNullOrEmpty(beginNextGuid) &&
                nodeViewMap.ContainsKey(graph.startNode.guid) &&
                nodeViewMap.TryGetValue(beginNextGuid, out var to))
            {   
                var from = nodeViewMap[graph.startNode.guid] as BeginNodeView;
                if (from != null && to != null)
                {
                    var input = GetInputPort(to);
                    if (input != null)
                    {
                        var edge = from.outputPort.ConnectTo(input);
                        AddElement(edge);
                    }
                }
            }
            
            
            foreach (var nodeView in graph.nodes)
            {
                if (nodeView is SentenceNode sentenceNode)
                {
                    string nextGuid = sentenceNode.nextNodeGuid;

                    if (!string.IsNullOrEmpty(nextGuid) && 
                        nodeViewMap.ContainsKey(sentenceNode.guid) && 
                        nodeViewMap.ContainsKey(nextGuid))

                    {
                        var fromView = nodeViewMap[sentenceNode.GUID] as SentenceNodeView;
                        var toView = nodeViewMap[nextGuid];

                        if (fromView != null &&
                            toView != null)
                        {
                            var input = GetInputPort(toView);
                            if (input != null)
                            {
                                var edge = fromView.outputPort.ConnectTo(input);
                                AddElement(edge);
                            }
                        }
                    }
                }
                else if (nodeView is AnswerNode answerNode)
                {
                    var fromView = nodeViewMap.GetValueOrDefault(answerNode.guid) as AnswerNodeView;
                    if (fromView == null) continue;
                    
                    if (!string.IsNullOrEmpty(answerNode.elseNextNodeGuid))
                    {
                        if (nodeViewMap.TryGetValue(answerNode.elseNextNodeGuid, out var toView))
                        {
                            var input = GetInputPort(toView);
                            if (input != null)
                            {
                                var edge = fromView.elsePort.ConnectTo(input);
                                AddElement(edge);
                            }
                        }
                    }


                    for (int i = 0; i < answerNode.answersCount; i++)
                    {
                        var nextGuid = answerNode.answers[i].nextNodeGuid;
                        if (string.IsNullOrEmpty(nextGuid)) continue;

                        if (nodeViewMap.TryGetValue(nextGuid, out var toView ))
                        {
                            var input = GetInputPort(toView);
                            if (input != null && i < fromView.outputPorts.Count)
                            {
                                var edge = fromView.outputPorts[i].ConnectTo(input);
                                AddElement(edge);

                            }
                        }
                    }
                } else if (nodeView is IfNode ifNode)
                {
                    string trueNextGuid = ifNode.trueNodeGuid;
                    string falseNextGuid = ifNode.falseNodeGuid;

                    if (!string.IsNullOrEmpty(trueNextGuid) &&
                        nodeViewMap.TryGetValue(ifNode.guid, out var fromTrueView) &&
                        nodeViewMap.TryGetValue(trueNextGuid, out var toTrueView))
                    {
                        var input = GetInputPort(toTrueView);
                        if (input != null)
                        {
                            var edge = (fromTrueView as IfNodeView).truePort.ConnectTo(input);
                            AddElement(edge);
                        }
                    }

                    if (!string.IsNullOrEmpty(falseNextGuid) &&
                        nodeViewMap.TryGetValue(ifNode.guid, out var fromFalseView) &&
                        nodeViewMap.TryGetValue(falseNextGuid, out var toFalseView))
                    {
                        var input = GetInputPort(toFalseView);
                        if (input != null)
                        {
                            var edge = (fromFalseView as IfNodeView).falsePort.ConnectTo(input);
                            AddElement(edge);
                        }
                    }
                } else if (nodeView is SimpleVariableMathNode simpleVariableMathNode)
                {
                    string nextGuid = simpleVariableMathNode.nextNodeGuid;

                    if (!string.IsNullOrEmpty(nextGuid) &&
                        nodeViewMap.TryGetValue(simpleVariableMathNode.guid, out var fromTrueView) &&
                        nodeViewMap.TryGetValue(nextGuid, out var toTrueView))
                    {
                        var input = GetInputPort(toTrueView);
                        if (input != null)
                        {
                            var edge = (fromTrueView switch
                            {
                                SetVariableNodeView setVariableNodeView => setVariableNodeView.outputPort,
                                AddVariableNodeView addVariableNodeView => addVariableNodeView.outputPort,
                                SubtractVariableNodeView subtractVariableNodeView => subtractVariableNodeView.outputPort,
                                MultiplyVariableNodeView multiplyVariableNodeView => multiplyVariableNodeView.outputPort,
                                DivideVariableNodeView divideVariableNodeView => divideVariableNodeView.outputPort,
                                _ => null
                            }).ConnectTo(input);
                            AddElement(edge);
                        }
                    }
                }
            }*/
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
            Undo.undoRedoPerformed -= OnUndoRedo;
        }
        
        private void OnUndoRedo()
        {
            // Reload the graph to reflect changes
            if (_dialogReference != null)
            {
                LoadGraph(_dialogReference, true);
                GraphChanged?.Invoke();
            }
        }
    }
}
