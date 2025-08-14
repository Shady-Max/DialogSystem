using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor
{
    public class CustomEdgeConnectorListener : IEdgeConnectorListener
    {
        private readonly CustomPort _port;
        
        public CustomEdgeConnectorListener(CustomPort port)
        {
            _port = port;
        }
        
        public void OnDropOutsidePort(Edge edge, Vector2 position) { }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            var outputPort = edge.output;
            var inputPort = edge.input;

            // Validate connection if either port is a CustomPort
            bool isConnectionValid = true;

            if (outputPort is CustomPort customOutput)
            {
                isConnectionValid = customOutput.IsConnectionAllowed(inputPort);
            }

            if (inputPort is CustomPort customInput && isConnectionValid)
            {
                isConnectionValid = customInput.IsConnectionAllowed(outputPort);
            }

            if (isConnectionValid)
            {
                if (inputPort.capacity == Port.Capacity.Single && inputPort.connected)
                {
                    // Remove existing connection for single capacity input port
                    var existingEdge = inputPort.connections.FirstOrDefault();
                    if (existingEdge != null)
                    {
                        (graphView as DialogGraphView)!.DeleteElements(new []{existingEdge});
                    }
                }

                if (outputPort.capacity == Port.Capacity.Single && outputPort.connected)
                {
                    // Remove existing connection for single capacity output port
                    var existingEdge = outputPort.connections.FirstOrDefault();
                    if (existingEdge != null)
                    {
                        (graphView as DialogGraphView)!.DeleteElements(new []{existingEdge});
                    }
                }
                
                var connectedEdge = outputPort.ConnectTo(inputPort);
                graphView.AddElement(connectedEdge);
                
                var graphViewChange = new GraphViewChange
                {
                    edgesToCreate = new List<Edge> { edge }
                };
        
                // Call the graph view's change handler
                if (graphView is DialogGraphView dialogGraphView)
                {
                    dialogGraphView.graphViewChanged(graphViewChange);
                }
            
                // Trigger events for custom ports
                if (outputPort is CustomPort customOut)
                {
                    customOut.TriggerPortConnect(edge);
                }

                if (inputPort is CustomPort customIn)
                {
                    customIn.TriggerPortConnect(edge);
                }
            }
        }
    }
}