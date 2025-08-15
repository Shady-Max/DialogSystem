using System.Collections.Generic;
using System.Linq;
using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor.Experimental.GraphView;

namespace ShadyMax.DialogSystem.Editor.ViewNodes
{
    public class AnswerNodeView : BaseNodeView<AnswerNodeEditor>
    {
        public Port inputPort;
        public List<CustomPort> conditionPorts = new List<CustomPort>();
        public List<Port> outputPorts = new List<Port>();
        public Port elsePort;
        
        private const string InKey = "In";
        private const string ElseKey = "Else Out";
        
        private readonly Dictionary<string, CustomPort> _conditionByKey = new Dictionary<string, CustomPort>();
        private readonly Dictionary<string, Port> _outputsByKey = new Dictionary<string, Port>();
        
        private bool _initialized;
        
        protected override string GetTitle() => "Answer Node";

        protected override void CreateInputPorts()
        {
            if (inputPort != null) return;
            
            inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(BaseNodeEditor));
            inputPort.portName = InKey;
            inputPort.name = InKey;
            inputContainer.Add(inputPort);
            
            EnsureConditionPortsCount(node.answerCount);
        }

        protected override void CreateOutputPorts()
        {
            if (elsePort == null)
            {
                elsePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BaseNodeEditor));
                elsePort.portName = ElseKey;
                elsePort.name = ElseKey;
                outputPorts.Add(elsePort);
                _outputsByKey[ElseKey] = elsePort;
                outputContainer.Add(elsePort);
            }
            
            EnsureAnswerPortsCount(node.answerCount);
        }

        protected override void CreateContent()
        {
            
        }

        protected override void RefreshUI()
        {
            if (!_initialized)
            {
                CreateInputPorts();
                CreateOutputPorts();
                CreateContent();
                _initialized = true;
            }
            else
            {
                GraphView.GraphChanged?.Invoke();
                EnsureConditionPortsCount(node.answerCount);
                EnsureAnswerPortsCount(node.answerCount);
            }

            // Update labels to reflect current indices
            UpdatePortLabels();

            RefreshPorts();
            RefreshExpandedState();
        }

        private void EnsureAnswerPortsCount(int desiredCount)
        {
            // Current answer ports are everything except elsePort (which is the first we added)
            // We keep them in outputPorts after elsePort.
            // Count how many "answer" ports we currently have:
            int currentCount = outputPorts.Count - 1; // excluding elsePort

            // Add missing ports
            for (int i = currentCount; i < desiredCount; i++)
            {
                var label = $"Answer {i + 1}";
                var p = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BaseNodeEditor));
                p.portName = label;
                p.name = label;
                outputPorts.Add(p);
                _outputsByKey[label] = p;
                outputContainer.Add(p);
            }

            // Remove extra ports (from tail only), deleting their edges as real model changes
            for (int i = currentCount - 1; i >= desiredCount; i--)
            {
                int indexInList = 1 + i; // +1 because elsePort is at index 0
                var p = outputPorts[indexInList];

                // Delete only edges connected to this port (valid model removal)
                var edges = p.connections?.ToList();
                if (edges != null && edges.Count > 0 && GraphView != null)
                {
                    GraphView.DeleteElements(edges);
                }

                _outputsByKey.Remove(p.name);
                outputPorts.RemoveAt(indexInList);
                outputContainer.Remove(p);
            }
        }
        
        private void EnsureConditionPortsCount(int desiredCount)
        {
            // Current answer ports are everything except elsePort (which is the first we added)
            // We keep them in outputPorts after elsePort.
            // Count how many "answer" ports we currently have:
            int currentCount = conditionPorts.Count; // excluding elsePort

            // Add missing ports
            for (int i = currentCount; i < desiredCount; i++)
            {
                var label = $"Condition {i + 1}";
                var p = CustomPort.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
                p.portName = label;
                p.name = label;
                conditionPorts.Add(p);
                _conditionByKey[label] = p;
                inputContainer.Add(p);
            }

            // Remove extra ports (from tail only), deleting their edges as real model changes
            for (int i = currentCount - 1; i >= desiredCount; i--)
            {
                int indexInList = i; // +1 because elsePort is at index 0
                var p = conditionPorts[indexInList];

                // Delete only edges connected to this port (valid model removal)
                var edges = p.connections?.ToList();
                if (edges != null && edges.Count > 0 && GraphView != null)
                {
                    GraphView.DeleteElements(edges);
                }

                _conditionByKey.Remove(p.name);
                outputPorts.RemoveAt(indexInList);
                inputContainer.Remove(p);
            }
        }

        private void UpdatePortLabels()
        {
            // Ensure Else label is correct
            if (elsePort != null)
            {
                elsePort.portName = ElseKey;
                elsePort.name = ElseKey;
                _outputsByKey[ElseKey] = elsePort;
            }

            // Keep answer labels consistent with their indices
            for (int i = 0; i < outputPorts.Count - 1; i++)
            {
                int indexInList = 1 + i; // skip elsePort
                var p = outputPorts[indexInList];
                var desired = $"Answer {i + 1}";

                if (p.name != desired)
                {
                    // Update dictionary key when renaming
                    _outputsByKey.Remove(p.name);
                    p.portName = desired;
                    p.name = desired;
                    _outputsByKey[desired] = p;
                }
                else
                {
                    // Keep dictionary mapping fresh
                    _outputsByKey[desired] = p;
                }
            }
            
            for (int i = 0; i < outputPorts.Count - 1; i++)
            {
                int indexInList = i;
                var p = conditionPorts[indexInList];
                var desired = $"Condition {i + 1}";

                if (p.name != desired)
                {
                    // Update dictionary key when renaming
                    _conditionByKey.Remove(p.name);
                    p.portName = desired;
                    p.name = desired;
                    _conditionByKey[desired] = p;
                }
                else
                {
                    // Keep dictionary mapping fresh
                    _conditionByKey[desired] = p;
                }
            }
        }

        
    }
}