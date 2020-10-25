using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Dlog.Runtime {
    public class DlogObject : ScriptableObject {
        // Graph data
        [SerializeField] public List<Node> Nodes;
        [SerializeField] public List<Edge> Edges;
        [SerializeField] public List<Property> Properties;
        [SerializeField] public Guid StartNode;
        public Guid CurrentNode;

        public Dictionary<Guid, Node> NodeDictionary;
        public Dictionary<Guid, Property> PropertyDictionary;

        public void BuildGraph() {
            PropertyDictionary = new Dictionary<Guid, Property>();
            foreach (var property in Properties) {
                PropertyDictionary.Add(property.Guid, property);
            }

            NodeDictionary = new Dictionary<Guid, Node>();
            foreach (var node in Nodes) {
                NodeDictionary.Add(node.Guid, node);
            }

            // Link node lines with actual properties, find previous node and actor node where necessary
            var propertyNodes = Nodes.Where(node => node.Type == NodeType.PROP).ToDictionary(node => node.Guid, node => node);
            
            // convert each conversation line reference from port to property lists using edge list
            foreach (var node in Nodes) {
                if (node.Type == NodeType.PROP) continue;
                foreach (var line in node.Lines) {
                    line.Checks = new List<Guid>();
                    line.Triggers = new List<Guid>();
                    foreach (var edge in Edges) {
                        // Find triggers
                        if (line.TriggerPort == edge.FromPort) {
                            var nodeGuid = edge.ToNode;
                            line.Triggers.Add(propertyNodes[nodeGuid].Temp_PropertyNodeGuid);
                        }
                        // Find checks, only for NPC nodes
                        if (node.Type == NodeType.NPC && line.CheckPort == edge.ToPort) {
                            var nodeGuid = edge.FromNode;
                            line.Checks.Add(propertyNodes[nodeGuid].Temp_PropertyNodeGuid);
                        }
                        // Find previous node
                        if (edge.ToNode == node.Guid && NodeDictionary[edge.FromNode].Type != NodeType.PROP)
                            node.Previous = edge.FromNode;

                        // Find actor node
                        if (edge.ToNode == node.Guid && node.Type == NodeType.NPC && propertyNodes.ContainsKey(edge.FromNode) && PropertyDictionary[propertyNodes[edge.FromNode].Temp_PropertyNodeGuid].Type == PropertyType.Actor)
                            node.ActorGuid = propertyNodes[edge.FromNode].Temp_PropertyNodeGuid;
                    }
                }
            }

            // Remove property nodes from Nodes and NodeDictionary
            var copyOfNodes = Nodes.ToList();
            copyOfNodes.ForEach(node => {
                if (node.Type != NodeType.PROP) return;
                NodeDictionary.Remove(node.Guid);
                Nodes.Remove(node);
            });

            // Find start node
            foreach (var node in Nodes) {
                if (node.Previous != Guid.Empty)
                    continue;
                
                if (StartNode != Guid.Empty) {
                    Debug.LogWarning("Multiple nodes without a previous node detected! Defaulting to the first one found to be the start node.");
                    continue;
                }

                StartNode = node.Previous;
            }

            CurrentNode = StartNode;
        }
    }
}