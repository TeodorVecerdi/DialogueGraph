using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Dlog {
    [Serializable]
    public class SerializedNode : ISerializationCallbackReceiver {
        [SerializeField] public string GUID;
        [SerializeField] public string Type;
        [SerializeField] public NodeDrawState DrawState;
        [SerializeField] public string NodeData;
        [SerializeField] public List<string> PortData;
        [NonSerialized] public Dictionary<string, Port> GuidPortDictionary;

        public EditorView EditorView;
        public AbstractNode Node;

        public SerializedNode(Type type, Rect position) {
            Type = type.FullName;
            DrawState.Position = position;
            DrawState.Expanded = true;
            GUID = Guid.NewGuid().ToString();
        }

        public void BuildNode(EditorView editorView, EdgeConnectorListener edgeConnectorListener, bool buildPortData = true) {
            EditorView = editorView;
            Node = (AbstractNode) Activator.CreateInstance(System.Type.GetType(Type));
            Node.InitializeNode(edgeConnectorListener);
            Node.GUID = GUID;
            Node.viewDataKey = GUID;
            Node.Owner = this;
            Node.SetExpandedWithoutNotify(DrawState.Expanded);
            Node.SetPosition(DrawState.Position);
            if (!string.IsNullOrEmpty(NodeData))
                Node.SetNodeData(NodeData);
            Node.Refresh();

            if (buildPortData)
                BuildPortData();
        }

        public void BuildPortData() {
            if ((Node.Ports == null || Node.Ports.Count == 0) && (PortData == null || PortData.Count == 0)) {
                return;
            }

            if ((PortData == null || PortData.Count == 0) && Node.Ports.Count != 0 || (PortData != null && PortData.Count != Node.Ports.Count && Node.Ports.Count != 0)) {
                // GET
                PortData = new List<string>();
                foreach (var port in Node.Ports) {
                    PortData.Add(port.viewDataKey);
                }
            } else {
                // SET
                if (PortData == null)
                    throw new InvalidDataException("Serialized port data somehow ended up as null when it was not supposed to.");
                for (var i = 0; i < PortData.Count; i++) {
                    Node.Ports[i].viewDataKey = PortData[i];
                }
            }

            // Build dictionary
            GuidPortDictionary = new Dictionary<string, Port>();
            foreach (var port in Node.Ports) {
                GuidPortDictionary.Add(port.viewDataKey, port);
            }
        }

        public void OnBeforeSerialize() {
            if (Node == null)
                return;
            Node.OnNodeSerialized();
            NodeData = Node.GetNodeData();
        }

        public void OnAfterDeserialize() {
            if (Node == null)
                return;
            Node.OnNodeDeserialized();
            if (!string.IsNullOrEmpty(NodeData))
                Node.SetNodeData(NodeData);
        }
    }
}