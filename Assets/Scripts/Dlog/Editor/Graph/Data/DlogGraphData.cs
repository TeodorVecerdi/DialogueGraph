using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Dlog {
    [Serializable]
    public class DlogGraphData : ISerializationCallbackReceiver {
        public DlogGraphObject Owner { get; set; }
        [SerializeField] public string AssetGuid;
        [SerializeField] public bool IsBlackboardVisible;

        [NonSerialized] private Dictionary<string, SerializedNode> nodeDictionary = new Dictionary<string, SerializedNode>();
        [SerializeField] private List<SerializedNode> nodes = new List<SerializedNode>();
        [NonSerialized] private List<SerializedNode> addedNodes = new List<SerializedNode>();
        [NonSerialized] private List<SerializedNode> removedNodes = new List<SerializedNode>();
        public List<SerializedNode> Nodes => nodes;
        public List<SerializedNode> AddedNodes => addedNodes;
        public List<SerializedNode> RemovedNodes => removedNodes;

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

        public void AddEdge(Edge edge) {
            var serializedEdge = new SerializedEdge {
                Input = edge.input.node.viewDataKey,
                Output = edge.output.node.viewDataKey,
                InputPort = edge.input.viewDataKey,
                OutputPort = edge.output.viewDataKey
            };
            AddEdge(serializedEdge);
        }

        public void AddEdge(SerializedEdge edge) {
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
            if (newIndex == currentIndex)
                return;
            properties.RemoveAt(currentIndex);
            if (newIndex > currentIndex)
                newIndex--;
            var isLast = newIndex == properties.Count;
            if (isLast)
                properties.Add(property);
            else
                properties.Insert(newIndex, property);
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

        public string DebugString() {
            var str = "Graph Data:\n";
            str += "\tNodes:\n";
            foreach (var node in nodes) {
                str += $"\t\t{node.GUID}\n";
            }

            str += "\tAdded Nodes:\n";
            foreach (var node in addedNodes) {
                str += $"\t\t{node.GUID}\n";
            }

            str += "\tRemoved Nodes:\n";
            foreach (var node in removedNodes) {
                str += $"\t\t{node.GUID}\n";
            }

            str += "\tEdges:\n";
            foreach (var edge in edges) {
                str += $"\t\t{edge.Input}->{edge.Output}\n";
            }

            str += "\tAdded Edges:\n";
            foreach (var edge in addedEdges) {
                str += $"\t\t{edge.Input}->{edge.Output}\n";
            }

            str += "\tRemoved Edges:\n";
            foreach (var edge in removedEdges) {
                str += $"\t\t{edge.Input}->{edge.Output}\n";
            }

            return str;
        }

        public void SanitizePropertyName(AbstractProperty property) {
            property.DisplayName = property.DisplayName.Trim();
            property.DisplayName = DlogUtility.SanitizeName(properties.Where(prop => prop.GUID != property.GUID).Select(prop => prop.DisplayName), "{0} ({1})", property.DisplayName);
        }

        public void SanitizePropertyReference(AbstractProperty property, string newReferenceName) {
            if (string.IsNullOrEmpty(newReferenceName))
                return;

            var name = newReferenceName.Trim();
            if (string.IsNullOrEmpty(name))
                return;

            property.OverrideReferenceName = DlogUtility.SanitizeName(properties.Where(prop => prop.GUID != property.GUID).Select(prop => prop.ReferenceName), "{0} ({1})", name);
        }
    }
}