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

        public DlogGraphView GraphView;
        public TempNode Node;

        public SerializedNode(Type type, Rect position) {
            Type = type.FullName;
            DrawState.Position = position;
            DrawState.Expanded = true;
            GUID = Guid.NewGuid().ToString();
        }

        public void BuildNode(DlogGraphView graphView, EdgeConnectorListener edgeConnectorListener) {
            GraphView = graphView;
            Node = (TempNode) Activator.CreateInstance(System.Type.GetType(Type));
            Node.InitializeNode(edgeConnectorListener);
            Node.GUID = GUID;
            Node.viewDataKey = GUID;
            Node.Owner = this;
            Node.SetExpandedWithoutNotify(DrawState.Expanded);
            Node.SetPosition(DrawState.Position);
            Node.Refresh();

            if ((PortData == null || PortData.Count == 0) && Node.Ports.Count != 0 || (PortData != null && PortData.Count != Node.Ports.Count)) {
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
            if (Node != null) {
                Node.OnNodeSerialized();
                NodeData = Node.GetNodeData();
            }
        }

        public void OnAfterDeserialize() {
            if (Node != null) {
                Node.OnNodeDeserialized();
                Node.SetNodeData(NodeData);
            }
        }
    }
}