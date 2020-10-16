using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dlog {
    [Serializable]
    public class CopyPasteData : ISerializationCallbackReceiver {
        [NonSerialized] private HashSet<SerializedNode> nodes = new HashSet<SerializedNode>();
        [NonSerialized] private HashSet<SerializedEdge> edges = new HashSet<SerializedEdge>();
        [NonSerialized] private HashSet<AbstractProperty> properties = new HashSet<AbstractProperty>();
        // these are the properties that don't get copied but are required by property nodes that get copied
        [NonSerialized] private HashSet<AbstractProperty> metaProperties = new HashSet<AbstractProperty>();
        
        [SerializeField] private List<SerializedNode> serializedNodes = new List<SerializedNode>();
        [SerializeField] private List<SerializedEdge> serializedEdges = new List<SerializedEdge>();
        [SerializeField] private List<SerializedProperty> serializedProperties = new List<SerializedProperty>();
        [SerializeField] private List<SerializedProperty> serializedMetaProperties = new List<SerializedProperty>();

        public IEnumerable<SerializedNode> Nodes => nodes;
        public IEnumerable<SerializedEdge> Edges => edges;
        public IEnumerable<SerializedNode> SerializedNodes => serializedNodes;
        public IEnumerable<SerializedEdge> SerializedEdges => serializedEdges;
        public IEnumerable<SerializedProperty> SerializedProperties => serializedProperties;
        public IEnumerable<SerializedProperty> SerializedMetaProperties => serializedMetaProperties;
        public IEnumerable<AbstractProperty> Properties => properties;
        public IEnumerable<AbstractProperty> MetaProperties => metaProperties;
        
        private EditorView editorView;

        public CopyPasteData(EditorView editorView, IEnumerable<SerializedNode> nodes, IEnumerable<SerializedEdge> edges, IEnumerable<AbstractProperty> properties, IEnumerable<AbstractProperty> metaProperties) {
            this.editorView = editorView;
            
            foreach (var node in nodes) {
                AddNode(node);
                foreach (var edge in GetAllEdgesForNode(node)) {
                    AddEdge(edge);
                }
            }

            foreach (var edge in edges) {
                AddEdge(edge);
            }

            foreach (var property in properties) {
                AddProperty(property);
            }

            foreach (var metaProperty in metaProperties) {
                AddMetaProperty(metaProperty);
            }
        }

        private void AddNode(SerializedNode node) {
            nodes.Add(node);
        }
        
        private void AddEdge(SerializedEdge edge) {
            edges.Add(edge);
        }
        private void AddProperty(AbstractProperty property) {
            properties.Add(property);
        }
        private void AddMetaProperty(AbstractProperty property) {
            metaProperties.Add(property);
        }

        public void OnBeforeSerialize() {
            serializedNodes = new List<SerializedNode>();
            foreach (var node in nodes) {
                serializedNodes.Add(node);
            }
            
            serializedEdges = new List<SerializedEdge>();
            foreach (var edge in edges) {
                serializedEdges.Add(edge);
            }
            
            serializedProperties = new List<SerializedProperty>();
            foreach (var property in properties) {
                serializedProperties.Add(new SerializedProperty(property));
            }

            serializedMetaProperties = new List<SerializedProperty>();
            foreach (var property in metaProperties) {
                serializedMetaProperties.Add(new SerializedProperty(property));
            }
        }

        public void OnAfterDeserialize() {
            nodes = new HashSet<SerializedNode>();
            edges = new HashSet<SerializedEdge>();
            properties = new HashSet<AbstractProperty>();
            metaProperties = new HashSet<AbstractProperty>();
            foreach (var node in serializedNodes) {
                nodes.Add(node);
            }
            foreach (var edge in serializedEdges) {
                edges.Add(edge);
            }
            foreach (var prop in serializedProperties) {
                properties.Add(prop.Deserialize());
            }
            foreach (var prop in serializedMetaProperties) {
                metaProperties.Add(prop.Deserialize());
            }
        }
        
        private IEnumerable<SerializedEdge> GetAllEdgesForNode(SerializedNode node) {
            var edges = new List<SerializedEdge>();
            foreach (var portConnections in node.GuidPortDictionary.Values.Select(port => port.connections)) {
                edges.AddRange(portConnections.Select(edge => edge.userData).OfType<SerializedEdge>());
            }
            return edges;
        }

        public static CopyPasteData FromJson(string json) {
            try {
                return JsonUtility.FromJson<CopyPasteData>(json);
            } catch {
                // ignored. just means json was not a CopyPasteData object
                return null;
            }
        }
    }
}