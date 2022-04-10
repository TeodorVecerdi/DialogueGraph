using System;
using System.Linq;
using DialogueGraph.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;

namespace DialogueGraph {
    public class PropertyNode : AbstractNode {
        private string propertyGuid;
        private string currentType;
        private EdgeConnectorListener edgeConnectorListener;

        public string PropertyGuid {
            get => propertyGuid;
            set {
                if (propertyGuid == value) return;
                propertyGuid = value;
                var property = Owner.EditorView.DlogObject.DlogGraph.Properties.FirstOrDefault(prop => prop.GUID == value);
                if (property == null) return;
                if(!string.IsNullOrEmpty(currentType))
                    RemoveFromClassList(currentType);
                currentType = property.Type.ToString();
                AddToClassList(currentType);
                CreatePorts(property);
            }
        }

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            Initialize("", EditorView.DefaultNodePosition);
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
            Port createdPort;
            switch (property.Type) {
                case PropertyType.Trigger:
                    createdPort = DlogPort.Create("", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, PortType.Trigger, false,edgeConnectorListener);
                    break;
                case PropertyType.Check:
                    createdPort = DlogPort.Create("", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Check, false, edgeConnectorListener);
                    break;
                case PropertyType.Actor:
                    createdPort = DlogPort.Create("", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Actor, false, edgeConnectorListener);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            AddPort(createdPort, false);
            if(createdPort.direction == Direction.Output)
                titleContainer.Add(createdPort);
            else {
                titleContainer.Insert(0, createdPort);
                titleContainer.AddToClassList("property-port-input");
            }
            Update(property);
            Refresh();
        }

        public void Update(AbstractProperty property) {
            title = property.DisplayName;
            Refresh();
        }
    }
}