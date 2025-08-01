using System;
using System.Collections.Generic;
using ShadyMax.DialogSystem.Editor.Nodes;
using UnityEngine;

namespace ShadyMax.DialogSystem.Editor
{
    [CreateAssetMenu(fileName = "Editor/DialogGraph", menuName = "Scriptable Objects/DialogGraph")]
    public class DialogGraphEditor : ScriptableObject
    {
        public List<BaseNodeEditor> nodes;
        public List<EdgeData> edges;
    }
}
