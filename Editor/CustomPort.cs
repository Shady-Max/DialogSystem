using System;
using System.Collections.Generic;
using System.Linq;
using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor
{
    public class CustomPort : Port
    {
        public event Action<Port, Edge> OnPortConnect;
        public event Action<Port, Edge> OnPortDisconnect;

        public Color FirstColor => _colors[0];
        
        private HashSet<Type> _allowedDataTypes = new HashSet<Type>();
        private readonly List<Color> _colors = new List<Color>();
        private RingElement _ringElement;
        
        protected CustomPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
            portColor = Color.clear;
            _ringElement = new RingElement();
            var connector = this.Q("connector");
            connector.Add(_ringElement);
            
            if (portType == typeof(int))
            {
                AddAllowedDataType<int>();
                AddAllowedDataType<float>();
            } 
            else if (portType == typeof(float))
            {
                AddAllowedDataType<float>();
                AddAllowedDataType<int>();
            } 
            else if (portType == typeof(bool))
            {
                AddAllowedDataType<bool>();
            } 
            else if (portType == typeof(string))
            {
                AddAllowedDataType<string>();
            }
            else
            {
                AddAllowedDataType<BaseNodeEditor>();
            }
        }

        
        public void SetColors(params Color[] colors)
        {
            portColor = FirstColor;
            _colors.Clear();
            _colors.AddRange(colors);
            _ringElement.SetColors(colors);
        }

        public new static CustomPort Create<TEdge>(Orientation orientation, Direction direction, Capacity capacity, Type type) where TEdge : Edge, new()
        {
            var port = new CustomPort(orientation, direction, capacity, type);
            var listener = new CustomEdgeConnectorListener(port);
            port.m_EdgeConnector = new EdgeConnector<TEdge>(listener);
            port.AddManipulator(port.m_EdgeConnector);
            return port;
        }
        
        public void AddAllowedDataType<T>()
        {
            if (typeof(T) == typeof(bool))
            {
                _colors.Add(new Color(255/255f, 197/255f, 0/255f, 1f));
            }
            else if (typeof(T) == typeof(string))
            {
                _colors.Add(new Color(8/255f, 255/255f, 93/255f, 1f));
            }
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(float))
            {
                _colors.Add(new Color(0/255f, 179/255f, 255/255f, 1f));
            }
            else
            {
                _colors.Add(Color.gray);
            }
            _allowedDataTypes.Add(typeof(T));
            SetColors(_colors.ToArray());
        }

        public void RemoveAllowedDataType<T>()
        {
            if (typeof(T) == typeof(bool))
            {
                _colors.Remove(new Color(255/255f, 197/255f, 0/255f, 1f));
            }
            else if (typeof(T) == typeof(string))
            {
                _colors.Remove(new Color(8/255f, 255/255f, 93/255f, 1f));
            }
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(float))
            {
                _colors.Remove(new Color(0/255f, 179/255f, 255/255f, 1f));
            }
            else
            {
                _colors.Remove(Color.gray);
            }
            _allowedDataTypes.Remove(typeof(T));
            SetColors(_colors.ToArray());
        }
        
        public HashSet<Type> GetAllowedDataTypes()
        {
            return new HashSet<Type>(_allowedDataTypes);
        }

        public bool HasAllowedDataType(Type type)
        {
            return _allowedDataTypes.Contains(type);
        }
        
        public bool IsConnectionAllowed(Port other)
        {
            if (_allowedDataTypes.Count == 0)
                return true;
            // If the other port is also a CustomPort, check for type compatibility
            if (other is CustomPort otherCustomPort)
            {
                // If other port has no restrictions, check if it accepts this port's types
                if (otherCustomPort._allowedDataTypes.Count == 0)
                    return true;
            
                // Check if there's any overlap between allowed types
                return _allowedDataTypes.Overlaps(otherCustomPort._allowedDataTypes);
            }
    
            // For regular ports, check if this port accepts the other port's type
            return _allowedDataTypes.Contains(other.portType);
        }
        
        public void TriggerPortConnect(Edge edge)
        {
            OnPortConnect?.Invoke(this, edge);
        }

        public void TriggerPortDisconnect(Edge edge)
        {
            OnPortDisconnect?.Invoke(this, edge);
        }
        
        public override void DisconnectAll()
        {
            var edgesToDisconnect = connections.ToList();
            base.DisconnectAll();

            foreach (var edge in edgesToDisconnect)
            {
                TriggerPortDisconnect(edge);
            } 
        }
    }
}