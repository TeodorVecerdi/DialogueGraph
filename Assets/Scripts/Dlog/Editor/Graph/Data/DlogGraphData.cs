using System;
using System.Collections.Generic;
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

        public void OnBeforeSerialize() {
            if(Owner != null)
                IsBlackboardVisible = Owner.IsBlackboardVisible;
        }

        public void OnAfterDeserialize() {
            
        }

        public void ClearChanges() {
            addedNodes.Clear();
            removedNodes.Clear();
            addedEdges.Clear();
            removedEdges.Clear();
        }

        public void ReplaceWith(DlogGraphData otherGraphData) {
            // Remove everything 
            foreach (var node in nodes) {
                RemoveNode(node);
            }
            // Add back everything
            foreach (var node in otherGraphData.nodes) {
                AddNode(node);
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
        }
    }
}