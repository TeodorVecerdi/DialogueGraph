using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    // Temporary class until I get to the implementation of nodes
    public abstract class TempNode : Node {
        public SerializedNode Owner { get; set; }
        public string GUID;
        public List<Port> Ports = new List<Port>();

        // public readonly List<TempPort> InputPorts = new List<TempPort>();
        // public readonly List<TempPort> OutputPorts = new List<TempPort>();
        // public readonly Dictionary<Port, TempPort> InputPortDictionary = new Dictionary<Port, TempPort>();
        // public readonly Dictionary<Port, TempPort> OutputPortDictionary = new Dictionary<Port, TempPort>();

        public override bool expanded {
            get => base.expanded;
            set {
                Owner.GraphView.DlogObject.RegisterCompleteObjectUndo("Expanded state changed");
                base.expanded = value;
                Owner.DrawState.Expanded = value;
            }
        }

        [Obsolete("Use TempPort.Create instead.", true)]
        public void InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type) {}

        [Obsolete("Use AddPort instead of manually adding ports to the container.", true)]
        public VisualElement inputContainer { get; private set; }

        [Obsolete("Use AddPort instead of manually adding ports to the container.", true)]
        public VisualElement outputContainer { get; private set; }

        protected void AddPort(Port port) {
            var isInput = port.direction == Direction.Input;
            if (isInput) {
                base.inputContainer.Add(port);
            } else {
                base.outputContainer.Add(port);
            }

            port.viewDataKey = Guid.NewGuid().ToString();
            Ports.Add(port);
        }

        public abstract void InitializeNode(EdgeConnectorListener edgeConnectorListener);

        protected void Initialize(string nodeTitle, Rect nodePosition) {
            base.title = nodeTitle;
            base.SetPosition(nodePosition);
            GUID = Guid.NewGuid().ToString();
            viewDataKey = GUID;
        }

        public void Refresh() {
            RefreshPorts();
            RefreshExpandedState();
        }

        public void SetExpandedWithoutNotify(bool value) {
            base.expanded = value;
        }

        public virtual JObject SetNodeData(string jsonData) {
            var root = JObject.Parse(jsonData);
            return root;
        }

        public virtual string GetNodeData() {
            var root = new JObject();
            return root.ToString(Formatting.None);
        }

        public virtual void OnNodeSerialized() { }
        public virtual void OnNodeDeserialized() { }

        public virtual void OnCreateFromSearchWindow(Vector2 nodePosition) { }
    }
}