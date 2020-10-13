using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Dlog {
    [Title("Property")]
    public class PropertyNode : AbstractNode {
        private string propertyGuid;
        private EdgeConnectorListener edgeConnectorListener;

        public string PropertyGuid {
            get => propertyGuid;
            set {
                if (propertyGuid == value) return;
                propertyGuid = value;
                var property = Owner.EditorView.DlogObject.DlogGraph.Properties.FirstOrDefault(prop => prop.GUID == value);
                if (property == null) return;
                CreatePorts(property);
            }
        }

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            Initialize("I AM PROPERTY", EditorView.DefaultNodePosition);
            this.edgeConnectorListener = edgeConnectorListener;
            Refresh();
        }

        
        public override string GetNodeData() {
            var root = new JObject();
            root["propertyGuid"] = propertyGuid;
            root.Merge(JObject.Parse(base.GetNodeData()));
            return root.ToString(Formatting.None);
        }

        public override void SetNodeData(string jsonData) {
            if(string.IsNullOrEmpty(jsonData)) return;
            base.SetNodeData(jsonData);
            var root = JObject.Parse(jsonData);
            PropertyGuid = root.Value<string>("propertyGuid");
        }

        private void CreatePorts(AbstractProperty property) {
            switch (property.Type) {
                case PropertyType.Trigger:
                    AddPort(DlogPort.Create(property.DisplayName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object), edgeConnectorListener));
                    break;
                case PropertyType.Check:
                    AddPort(DlogPort.Create(property.DisplayName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object), edgeConnectorListener));
                    break;
                case PropertyType.Actor:
                    AddPort(DlogPort.Create(property.DisplayName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object), edgeConnectorListener));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Refresh();
        }
    }
}