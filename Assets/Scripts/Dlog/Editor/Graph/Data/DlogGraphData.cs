using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

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

        public void OnBeforeSerialize() {
            if (Owner != null)
                IsBlackboardVisible = Owner.IsBlackboardVisible;
        }

        public void OnAfterDeserialize() {
            nodes.ForEach(node => nodeDictionary.Add(node.GUID, node));
        }

        public void ClearChanges() {
            addedNodes.Clear();
            removedNodes.Clear();
            addedEdges.Clear();
            removedEdges.Clear();
        }

        public void ReplaceWith(DlogGraphData otherGraphData) {
            // Remove everything 
            var removedNodesGuid = new List<string>();
            removedNodesGuid.AddRange(nodes.Select(node => node.GUID));
            foreach (var node in removedNodesGuid) {
                RemoveNode(nodeDictionary[node]);
            }

            // Add back everything
            foreach (var node in otherGraphData.nodes) {
                AddNode(node);
            }

            foreach (var edge in otherGraphData.edges) {
                AddEdge(edge);
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
    }
}