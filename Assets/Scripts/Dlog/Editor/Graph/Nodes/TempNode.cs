using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Dlog {
    // Temporary class until I get to the implementation of nodes
    public abstract class TempNode : Node {
        public SerializedNode Owner { get; set; }
        public string GUID;

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

        public virtual void SetNodeData(string jsonData) {
            var root = JObject.Parse(jsonData);
            base.expanded = root.Value<bool>("expanded");
        }

        public virtual string GetNodeData() {
            var root = new JObject();
            root["expanded"] = base.expanded;
            return root.ToString(Formatting.None);
        }
        
        public virtual void OnNodeSerialized() {}
        public virtual void OnNodeDeserialized() {}
        
        public virtual void OnCreateFromSearchWindow(Vector2 nodePosition) {}
    }
}