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

        protected EdgeConnectorListener EdgeConnectorListener;

        public override bool expanded {
            get => base.expanded;
            set {
                Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Expanded state changed");
                base.expanded = value;
                Owner.DrawState.Expanded = value;
            }
        }

        // [Obsolete("Use TempPort.Create instead.", true)]
        // public new void InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type) { }

        // ReSharper disable once InconsistentNaming, UnusedAutoPropertyAccessor.Local
        [Obsolete("Use AddPort instead of manually adding ports to the container. Only use this if you're adding custom items to the container.", false)]
        protected new VisualElement inputContainer => base.inputContainer;

        // ReSharper disable once InconsistentNaming, UnusedAutoPropertyAccessor.Local
        [Obsolete("Use AddPort instead of manually adding ports to the container. Only use this if you're adding custom items to the container.", false)]
        protected new VisualElement outputContainer => base.outputContainer;

        protected void AddPort(Port port, bool alsoAddToHierarchy = true) {
            Ports.Add(port);
            
            if(!alsoAddToHierarchy) return;
            var isInput = port.direction == Direction.Input;
            if (isInput) {
                base.inputContainer.Add(port);
            } else {
                base.outputContainer.Add(port);
            }

        }

        public virtual void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            EdgeConnectorListener = edgeConnectorListener;
        }

        protected void Initialize(string nodeTitle, Rect nodePosition) {
            base.title = nodeTitle;
            base.SetPosition(nodePosition);
            GUID = Guid.NewGuid().ToString();
            viewDataKey = GUID;
            this.AddStyleSheet("Styles/Node/Node");
            this.InjectCustomStyle();
        }

        public void Refresh() {
            RefreshPorts();
            RefreshExpandedState();
        }

        public void SetExpandedWithoutNotify(bool value) {
            base.expanded = value;
        }

        public virtual void SetNodeData(string jsonData) { }

        public virtual string GetNodeData() {
            var root = new JObject();
            return root.ToString(Formatting.None);
        }

        public virtual void OnNodeSerialized() { }
        public virtual void OnNodeDeserialized() { }
    }
}