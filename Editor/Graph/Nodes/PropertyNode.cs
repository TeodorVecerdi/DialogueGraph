using System;
using System.Linq;
using DialogueGraph.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;

namespace DialogueGraph {
    public class PropertyNode : AbstractNode {
        private string m_PropertyGuid;
        private string m_CurrentType;
        private EdgeConnectorListener m_EdgeConnectorListener;

        public string PropertyGuid {
            get => m_PropertyGuid;
            set {
                if (m_PropertyGuid == value) return;
                m_PropertyGuid = value;

                AbstractProperty property = Owner.EditorView.DlogObject.GraphData.Properties.FirstOrDefault(prop => prop.GUID == value);
                if (property == null) return;

                if (!string.IsNullOrEmpty(m_CurrentType)) {
                    RemoveFromClassList(m_CurrentType);
                }

                m_CurrentType = property.Type.ToString();
                AddToClassList(m_CurrentType);
                CreatePorts(property);
            }
        }

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            Initialize("", EditorView.DefaultNodePosition);
            m_EdgeConnectorListener = edgeConnectorListener;
            Refresh();
        }

        public override string GetNodeData() {
            JObject root = new() {
                ["propertyGuid"] = m_PropertyGuid,
            };

            string baseNodeData = base.GetNodeData();
            if (baseNodeData != "{}") {
                root.Merge(JObject.Parse(baseNodeData));
            }

            return root.ToString(Formatting.None);
        }

        public override void SetNodeData(string jsonData) {
            if (string.IsNullOrEmpty(jsonData)) return;
            base.SetNodeData(jsonData);

            JObject root = JObject.Parse(jsonData);
            PropertyGuid = root.Value<string>("propertyGuid");
        }

        private void CreatePorts(AbstractProperty property) {
            Port createdPort = property.Type switch {
                PropertyType.Trigger => DlogPort.Create("", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, PortType.Trigger, false, m_EdgeConnectorListener),
                PropertyType.Check => DlogPort.Create("", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Check, false, m_EdgeConnectorListener),
                PropertyType.Actor => DlogPort.Create("", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Actor, false, m_EdgeConnectorListener),
                _ => throw new ArgumentOutOfRangeException(nameof(property.Type), property.Type, null),
            };

            AddPort(createdPort, false);
            if (createdPort.direction == Direction.Output) {
                titleContainer.Add(createdPort);
            } else {
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