using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public abstract class AbstractNode : Node {
        public SerializedNode Owner { get; set; }
        public string GUID;
        public readonly List<Port> Ports = new List<Port>();

        public override bool expanded {
            get => base.expanded;
            set {
                Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Expanded state changed");
                base.expanded = value;
                Owner.DrawState.Expanded = value;
            }
        }

        [Obsolete("Use TempPort.Create instead.", true)]
        public new void InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type) {}

        [Obsolete("Use AddPort instead of manually adding ports to the container.", true)]
        // ReSharper disable once InconsistentNaming, UnusedAutoPropertyAccessor.Local
        public new VisualElement inputContainer { get; private set; }

        [Obsolete("Use AddPort instead of manually adding ports to the container.", true)]
        // ReSharper disable once InconsistentNaming, UnusedAutoPropertyAccessor.Local
        public new VisualElement outputContainer { get; private set; }

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

        public virtual void SetNodeData(string jsonData) {
            if(jsonData == null) return;
            var root = JObject.Parse(jsonData);
        }

        public virtual string GetNodeData() {
            var root = new JObject();
            return root.ToString(Formatting.None);
        }

        public virtual void OnNodeSerialized() { }
        public virtual void OnNodeDeserialized() { }
    }
}