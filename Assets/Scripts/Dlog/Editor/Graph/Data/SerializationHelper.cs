using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public static class SerializationHelper {
        public static SerializedNode SerializeNode(TempNode node) {
            node.OnNodeSerialized();
            return node.Owner;
        }

        public static List<SerializedNode> SerializeNodes(List<TempNode> nodes) {
            var serializedNodes = new List<SerializedNode>();
            serializedNodes.AddRange(nodes.Select(SerializeNode));
            return serializedNodes;
        }

        public static TempNode DeserializeNode(SerializedNode serializedNode) {
            var type = Type.GetType("CodeGraph.Editor." + serializedNode.Type);
            var deserializedNode = (TempNode) Activator.CreateInstance(type);
            deserializedNode.GUID = serializedNode.GUID;
            deserializedNode.SetNodeData(serializedNode.NodeData);
            deserializedNode.SetPosition(new Rect(serializedNode.Position, DlogGraphView.DefaultNodeSize));
            deserializedNode.OnNodeDeserialized();
            deserializedNode.Refresh();
            return deserializedNode;
        }

        public static List<TempNode> DeserializeNodes(List<SerializedNode> nodes) {
            var deserializedNodes = new List<TempNode>();
            deserializedNodes.AddRange(nodes.Select(DeserializeNode));
            return deserializedNodes;
        }

        public static SerializedEdge SerializeEdge(Edge edge) {
            var inputNode = edge.input.node as TempNode;
            var outputNode = edge.output.node as TempNode;
            /*for (var i = 0; i < inputNode.InputPorts.Count; i++) {
                if (edge.input == inputNode.InputPorts[i].PortReference) {
                    inputPortIndex = i;
                    break;
                }
            }

            
            for (var i = 0; i < outputNode.OutputPorts.Count; i++) {
                if (edge.output == outputNode.OutputPorts[i].PortReference) {
                    outputPortIndex = i;
                    break;
                }
            }*/
            // TODO: Add input/output port containers
            var inputPortIndex = -1;
            var outputPortIndex = -1;
            return new SerializedEdge {
                FromGUID = outputNode.GUID,
                FromIndex = outputPortIndex,
                ToGUID = inputNode.GUID,
                ToIndex = inputPortIndex
            };
        }
        
        public static List<SerializedEdge> SerializeEdges(List<Edge> edges) {
            var serializeEdges = new List<SerializedEdge>();
            serializeEdges.AddRange(edges.Select(SerializeEdge));
            return serializeEdges;
        }

        public static Edge DeserializeAndLinkEdge(SerializedEdge edge, List<TempNode> nodes) {
            var sourceNode = nodes.First(x => x.GUID == edge.FromGUID);
            var targetNode = nodes.First(x => x.GUID == edge.ToGUID);
            var deserializedEdge = LinkNodesTogether(sourceNode.outputContainer[edge.FromIndex].Q<Port>(),
                targetNode.inputContainer[edge.ToIndex].Q<Port>());
            return deserializedEdge;
        }

        public static List<Edge> DeserializeAndLinkEdges(List<SerializedEdge> edges, List<TempNode> nodes) {
            var deserializedEdges = new List<Edge>();
            deserializedEdges.AddRange(edges.Select(edge => DeserializeAndLinkEdge(edge, nodes)));
            return deserializedEdges;
        }
        
        public static Edge LinkNodesTogether(Port outputSocket, Port inputSocket) {
            var edge = new Edge {
                output = outputSocket,
                input = inputSocket
            };
            edge.input.Connect(edge);
            edge.output.Connect(edge);
            return edge;
        }
    }
}