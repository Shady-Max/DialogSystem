using ShadyMax.DialogSystem.Runtime.Nodes;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor.Nodes
{
    public class BaseNodeEditor : BaseNode
    {
        private Vector2 _position;
        
        public Vector2 Position { get => _position; set => _position = value; }
    }
}