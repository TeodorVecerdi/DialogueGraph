using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Dlog {
    [Serializable]
    public class SerializedNode : ISerializationCallbackReceiver {
        [SerializeField] public string GUID;
        [SerializeField] public string Type;
        [SerializeField] public bool Expanded;
        [SerializeField] public Vector2 Position;
        [SerializeField] public string NodeData;

        private Type type;
        public TempNode Node;

        public SerializedNode(Type type, Vector2 position) {
            Type = type.Name;
            Position = position;
            GUID = Guid.NewGuid().ToString();
            Expanded = true;
            Node = (TempNode) Activator.CreateInstance(type);
            Node.GUID = GUID;
            Node.viewDataKey = GUID;
            Node.Owner = this;
            Node.SetPosition(new Rect(Position, DlogGraphView.DefaultNodeSize));
            Node.OnCreateFromSearchWindow(position);
        }

        public void OnBeforeSerialize() {
            var stackFrames = new StackTrace().GetFrames();
            var s = $"SerializedNode::OnBeforeSerialize called from: {stackFrames[1].GetFileName()}::{stackFrames[1].GetMethod().Name}";
            for (int i = 2; i < stackFrames.Length; i++) {
                s += $", {stackFrames[1].GetFileName()}::{stackFrames[1].GetMethod().Name}";
            }
            Debug.Log(s);
            Expanded = Node.expanded;
            Position = Node.GetPosition().position;
            NodeData = Node.GetNodeData();
            Node.OnNodeSerialized();
        }

        public void OnAfterDeserialize() {
            
        }
    }
}