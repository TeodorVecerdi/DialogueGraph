using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Dlog {
    [Serializable]
    public class DlogGraphData : ISerializationCallbackReceiver {
        public DlogGraphObject Owner { get; set; }
        [SerializeField] public string AssetGuid;
        [SerializeField] public bool IsBlackboardVisible;
        [SerializeField] public string DialogueGraphVersion;


        [NonSerialized] private Dictionary<string, SerializedNode> nodeDictionary = new Dictionary<string, SerializedNode>();
        [SerializeField] private List<SerializedNode> nodes = new List<SerializedNode>();
        [NonSerialized] private List<SerializedNode> addedNodes = new List<SerializedNode>();
        [NonSerialized] private List<SerializedNode> removedNodes = new List<SerializedNode>();
        [NonSerialized] private List<SerializedNode> pastedNodes = new List<SerializedNode>();
        public List<SerializedNode> Nodes => nodes;
        public List<SerializedNode> AddedNodes => addedNodes;
        public List<SerializedNode> RemovedNodes => removedNodes;
        public List<SerializedNode> PastedNodes => pastedNodes;

        [SerializeField] private List<SerializedEdge> edges = new List<SerializedEdge>();
        [NonSerialized] private List<SerializedEdge> addedEdges = new List<SerializedEdge>();
        [NonSerialized] private List<SerializedEdge> removedEdges = new List<SerializedEdge>();
        public List<SerializedEdge> Edges => edges;
        public List<SerializedEdge> AddedEdges => addedEdges;
        public List<SerializedEdge> RemovedEdges => removedEdges;

        [NonSerialized] private List<AbstractProperty> properties = new List<AbstractProperty>();
        [NonSerialized] private List<AbstractProperty> addedProperties = new List<AbstractProperty>();
        [NonSerialized] private List<AbstractProperty> removedProperties = new List<AbstractProperty>();
        [NonSerialized] private List<AbstractProperty> movedProperties = new List<AbstractProperty>();
        [SerializeField] private List<SerializedProperty> serializedProperties = new List<SerializedProperty>();
        public List<AbstractProperty> Properties => properties;
        public List<AbstractProperty> AddedProperties => addedProperties;
        public List<AbstractProperty> RemovedProperties => removedProperties;
        public List<AbstractProperty> MovedProperties => movedProperties;

        [NonSerialized] private List<SerializedNode> nodeSelectionQueue = new List<SerializedNode>();
        [NonSerialized] private List<SerializedEdge> edgeSelectionQueue = new List<SerializedEdge>();
        public List<SerializedNode> NodeSelectionQueue => nodeSelectionQueue;
        public List<SerializedEdge> EdgeSelectionQueue => edgeSelectionQueue;

        public void OnBeforeSerialize() {
            if (Owner != null)
                IsBlackboardVisible = Owner.IsBlackboardVisible;

            serializedProperties.Clear();
            foreach (var property in properties) {
                serializedProperties.Add(new SerializedProperty(property));
            }
        }

        public void OnAfterDeserialize() {
            nodes.ForEach(node => nodeDictionary.Add(node.GUID, node));
            serializedProperties.ForEach(prop => AddProperty(prop.Deserialize()));
        }

        public void ClearChanges() {
            addedNodes.Clear();
            removedNodes.Clear();
            addedEdges.Clear();
            removedEdges.Clear();
            addedProperties.Clear();
            removedProperties.Clear();
            movedProperties.Clear();
            nodeSelectionQueue.Clear();
            edgeSelectionQueue.Clear();
        }

        public void ReplaceWith(DlogGraphData otherGraphData) {
            // Remove everything
            var removedNodesGuid = new List<string>();
            removedNodesGuid.AddRange(nodes.Select(node => node.GUID));
            foreach (var node in removedNodesGuid) {
                RemoveNode(nodeDictionary[node]);
            }

            var removedProperties = new List<AbstractProperty>(properties);
            foreach (var prop in removedProperties)
                RemoveProperty(prop);

            // Add back everything
            foreach (var node in otherGraphData.nodes) {
                AddNode(node);
            }

            foreach (var edge in otherGraphData.edges) {
                AddEdge(edge);
            }

            foreach (var property in otherGraphData.properties) {
                AddProperty(property);
            }
        }

        public void AddNode(SerializedNode node) {
            nodeDictionary.Add(node.GUID, node);
            nodes.Add(node);
            addedNodes.Add(node);
        }

        public void RemoveNode(SerializedNode node) {
            if (!nodeDictionary.ContainsKey(node.GUID))
                throw new InvalidOperationException($"Cannot remove node ({node.GUID}) because it doesn't exist.");

            nodes.Remove(node);
            nodeDictionary.Remove(node.GUID);
            removedNodes.Add(node);

            edges.Where(edge => edge.Input == node.GUID || edge.Output == node.GUID).ToList().ForEach(RemoveEdge);
        }

        public bool HasEdge(Edge edge) {
            var serializedEdge = new SerializedEdge {
                Input = edge.input.node.viewDataKey,
                Output = edge.output.node.viewDataKey,
                InputPort = edge.input.viewDataKey,
                OutputPort = edge.output.viewDataKey
            };
            return Edges.Any(edge1 => edge1.Input == serializedEdge.Input && edge1.Output == serializedEdge.Output && edge1.InputPort == serializedEdge.InputPort && edge1.OutputPort == serializedEdge.OutputPort);
        }

        public void AddEdge(Edge edge) {
            var serializedEdge = new SerializedEdge {
                Input = edge.input.node.viewDataKey,
                Output = edge.output.node.viewDataKey,
                InputPort = edge.input.viewDataKey,
                OutputPort = edge.output.viewDataKey,
                InputCapacity  = edge.input.capacity,
                OutputCapacity = edge.output.capacity
            };
            AddEdge(serializedEdge);
        }

        public void AddEdge(SerializedEdge edge) {
            if (edge.InputCapacity == Port.Capacity.Single) {
                // Remove all edges with the same port
                var temp = new List<SerializedEdge>();
                temp.AddRange(edges.Where(edge1 => edge1.InputPort == edge.InputPort));
                temp.ForEach(RemoveEdge);
            }

            if (edge.OutputCapacity == Port.Capacity.Single) {
                // Remove all edges with the same port
                var temp = new List<SerializedEdge>();
                temp.AddRange(edges.Where(edge1 => edge1.OutputPort == edge.OutputPort));
                temp.ForEach(RemoveEdge);
            }

            edges.Add(edge);
            addedEdges.Add(edge);
        }

        public void RemoveEdge(SerializedEdge edge) {
            edges.Remove(edge);
            removedEdges.Add(edge);
        }

        public void AddProperty(AbstractProperty property) {
            if (property == null) return;
            if (properties.Contains(property)) return;
            properties.Add(property);
            addedProperties.Add(property);
        }

        public void RemoveProperty(AbstractProperty property) {
            var propertyNodes = nodes.FindAll(node => node.Node is PropertyNode propertyNode && propertyNode.PropertyGuid == property.GUID);
            foreach (var node in propertyNodes)
                RemoveNode(node);

            if (properties.Remove(property)) {
                removedProperties.Add(property);
                addedProperties.Remove(property);
                movedProperties.Remove(property);
            }
        }

        public void MoveProperty(AbstractProperty property, int newIndex) {
            if (newIndex > properties.Count || newIndex < 0)
                throw new ArgumentException("New index is not within properties list.");
            var currentIndex = properties.IndexOf(property);
            if (currentIndex == -1)
                throw new ArgumentException("Property is not in graph.");
            if (newIndex == currentIndex) {
                Debug.Log($"New index is the same as current index {newIndex} == {currentIndex}");
                return;
            }
            properties.RemoveAt(currentIndex);
            if (newIndex > currentIndex)
                newIndex--;
            var isLast = newIndex == properties.Count;
            if (isLast) {
                Debug.Log($"New index is the last index new:{newIndex} current:{currentIndex}");
                properties.Add(property);
            } else {
                Debug.Log($"new:{newIndex} current:{currentIndex}");
                properties.Insert(newIndex, property);
            }
            if (!movedProperties.Contains(property))
                movedProperties.Add(property);
        }

        public void RemoveElements(List<SerializedNode> nodes, List<SerializedEdge> edges) {
            foreach (var edge in edges) {
                RemoveEdge(edge);
            }

            foreach (var node in nodes) {
                RemoveNode(node);
            }
        }

        public void QueueSelection(List<SerializedNode> nodes, List<SerializedEdge> edges) {
            nodeSelectionQueue.AddRange(nodes);
            edgeSelectionQueue.AddRange(edges);
        }

        public void SanitizePropertyName(AbstractProperty property) {
            property.DisplayName = property.DisplayName.Trim();
            property.DisplayName = DialogueGraphUtility.SanitizeName(properties.Where(prop => prop.GUID != property.GUID).Select(prop => prop.DisplayName), "{0} ({1})", property.DisplayName);
        }

        public void SanitizePropertyReference(AbstractProperty property, string newReferenceName) {
            if (string.IsNullOrEmpty(newReferenceName))
                return;

            var name = newReferenceName.Trim();
            if (string.IsNullOrEmpty(name))
                return;

            property.OverrideReferenceName = DialogueGraphUtility.SanitizeName(properties.Where(prop => prop.GUID != property.GUID).Select(prop => prop.ReferenceName), "{0} ({1})", name);
        }

        public void Paste(CopyPasteData copyPasteData, List<SerializedNode> remappedNodes, List<SerializedEdge> remappedEdges) {
            var nodeGuidMap = new Dictionary<string, string>();
            var portGuidMap = new Dictionary<string, string>();
            foreach (var node in copyPasteData.Nodes) {
                var oldGuid = node.GUID;
                var newGuid = Guid.NewGuid().ToString();
                node.GUID = newGuid;
                nodeGuidMap[oldGuid] = newGuid;
                for (var i = 0; i < node.PortData.Count; i++) {
                    var newPortGuid = Guid.NewGuid().ToString();
                    var oldPortGuid = node.PortData[i];
                    portGuidMap[oldPortGuid] = newPortGuid;

                    node.PortData[i] = newPortGuid;
                }

                // Ugly magic to change dynamic port guid data
                if (node.Type == typeof(SelfNode).FullName) {
                    var data = JObject.Parse(node.NodeData);
                    var lines = JsonConvert.DeserializeObject<List<LineDataSelf>>(data.Value<string>("lines"));

                    foreach (var currLine in lines) {
                        currLine.PortGuidA = portGuidMap[currLine.PortGuidA];
                        currLine.PortGuidB = portGuidMap[currLine.PortGuidB];
                    }

                    data["lines"] = new JValue(JsonConvert.SerializeObject(lines));
                    node.NodeData = data.ToString(Formatting.None);
                } else if (node.Type == typeof(NpcNode).FullName) {
                    var data = JObject.Parse(node.NodeData);
                    var lines = JsonConvert.DeserializeObject<List<LineDataNpc>>(data.Value<string>("lines"));

                    foreach (var currLine in lines) {
                        currLine.PortGuidA = portGuidMap[currLine.PortGuidA];
                        currLine.PortGuidB = portGuidMap[currLine.PortGuidB];
                        currLine.PortGuidC = portGuidMap[currLine.PortGuidC];
                    }

                    data["lines"] = new JValue(JsonConvert.SerializeObject(lines));
                    node.NodeData = data.ToString(Formatting.None);
                }

                // offset the pasted node slightly so it's not on top of the original one
                var drawState = node.DrawState;
                var position = drawState.Position;
                position.x += 30;
                position.y += 30;
                drawState.Position = position;
                node.DrawState = drawState;
                remappedNodes.Add(node);
                AddNode(node);

                // add the node to the pasted node list
                pastedNodes.Add(node);
            }

            foreach (var edge in copyPasteData.Edges) {
                if ((nodeGuidMap.ContainsKey(edge.Input) && nodeGuidMap.ContainsKey(edge.Output)) && (portGuidMap.ContainsKey(edge.InputPort) && portGuidMap.ContainsKey(edge.OutputPort))) {
                    var remappedOutputGuid = nodeGuidMap.ContainsKey(edge.Output) ? nodeGuidMap[edge.Output] : edge.Output;
                    var remappedInputGuid = nodeGuidMap.ContainsKey(edge.Input) ? nodeGuidMap[edge.Input] : edge.Input;
                    var remappedOutputPortGuid = portGuidMap.ContainsKey(edge.OutputPort) ? portGuidMap[edge.OutputPort] : edge.OutputPort;
                    var remappedInputPortGuid = portGuidMap.ContainsKey(edge.InputPort) ? portGuidMap[edge.InputPort] : edge.InputPort;
                    var remappedEdge = new SerializedEdge {
                        Input = remappedInputGuid,
                        Output = remappedOutputGuid,
                        InputPort = remappedInputPortGuid,
                        OutputPort = remappedOutputPortGuid,
                        InputCapacity = edge.InputCapacity,
                        OutputCapacity = edge.OutputCapacity
                    };
                    remappedEdges.Add(remappedEdge);
                    AddEdge(remappedEdge);
                }
            }
        }
    }
}