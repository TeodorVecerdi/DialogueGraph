using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dlog {
    [Serializable]
    public class CopyPasteData : ISerializationCallbackReceiver {
        [SerializeField] private HashSet<SerializedNode> nodes = new HashSet<SerializedNode>();
        [SerializeField] private HashSet<SerializedEdge> edges = new HashSet<SerializedEdge>();
        [SerializeField] private HashSet<SerializedProperty> serializedProperties = new HashSet<SerializedProperty>();
        [SerializeField] private HashSet<SerializedProperty> serializedMetaProperties = new HashSet<SerializedProperty>();

        public IEnumerable<SerializedNode> Nodes => nodes;
        public IEnumerable<SerializedEdge> Edges => edges;
        public IEnumerable<SerializedProperty> Properties => serializedProperties;
        public IEnumerable<SerializedProperty> MetaProperties => serializedMetaProperties;

        [NonSerialized]
        private HashSet<AbstractProperty> properties = new HashSet<AbstractProperty>();

        // these are the properties that don't get copied but are required by property nodes that get copied
        [NonSerialized] private HashSet<AbstractProperty> metaProperties = new HashSet<AbstractProperty>();

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
            serializedProperties.Clear();
            foreach (var property in properties) {
                serializedProperties.Add(new SerializedProperty(property));
            }

            serializedMetaProperties.Clear();
            foreach (var property in metaProperties) {
                serializedMetaProperties.Add(new SerializedProperty(property));
            }
        }

        public void OnAfterDeserialize() {
            serializedProperties.ToList().ForEach(prop => properties.Add(prop.Deserialize()));
            serializedMetaProperties.ToList().ForEach(prop => metaProperties.Add(prop.Deserialize()));
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