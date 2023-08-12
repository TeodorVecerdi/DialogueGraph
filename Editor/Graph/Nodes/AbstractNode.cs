using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueGraph {
    public abstract class AbstractNode : Node {
        public SerializedNode Owner { get; set; }
        public string GUID { get; set; }
        public List<Port> Ports { get; } = new();

        protected EdgeConnectorListener EdgeConnectorListener { get; private set; }

        public override bool expanded {
            get => base.expanded;
            set {
                Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Expanded state changed");
                base.expanded = value;
                Owner.DrawState.Expanded = value;
            }
        }

        protected void AddPort(Port port, bool alsoAddToHierarchy = true) {
            Ports.Add(port);

            if(!alsoAddToHierarchy) return;
            if (port.direction == Direction.Input) {
                this.inputContainer.Add(port);
            } else {
                this.outputContainer.Add(port);
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
            InjectCustomStyle();
        }

        protected virtual void InjectCustomStyle() {
            VisualElement border = this.Q("node-border");
            StyleEnum<Overflow> overflowStyle = border.style.overflow;
            overflowStyle.value = Overflow.Visible;
            border.style.overflow = overflowStyle;

            VisualElement selectionBorder = this.Q("selection-border");
            selectionBorder.SendToBack();
        }

        public void Refresh() {
            RefreshPorts();
            RefreshExpandedState();
        }

        public void SetExpandedWithoutNotify(bool value) {
            base.expanded = value;
        }

        public virtual void SetNodeData(string jsonData) { }
        public virtual string GetNodeData() => "{}";
        public virtual void OnNodeSerialized() { }
        public virtual void OnNodeDeserialized() { }
    }
}