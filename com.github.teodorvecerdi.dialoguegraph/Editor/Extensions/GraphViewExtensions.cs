using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public static class GraphViewExtensions {
        public static void InsertCopyPasteData(this DlogGraphView graphView, CopyPasteData copyPasteData) {
            if (copyPasteData == null) return;
            foreach (var property in copyPasteData.Properties) {
                var copy = property.Copy();
                graphView.DlogGraph.SanitizePropertyName(copy);
                graphView.DlogGraph.SanitizePropertyReference(copy, property.OverrideReferenceName);
                graphView.DlogGraph.AddProperty(copy);

                var dependentNodes = copyPasteData.Nodes.Where(node => node.Type == typeof(PropertyNode).FullName);
                foreach (var node in dependentNodes) {
                    var root = JObject.Parse(node.NodeData);
                    root["propertyGuid"] = copy.GUID;
                    node.NodeData = root.ToString(Formatting.None);
                }
            }
            
            var remappedNodes = new List<SerializedNode>();
            var remappedEdges = new List<SerializedEdge>();
            graphView.DlogGraph.Paste(copyPasteData, remappedNodes, remappedEdges);

            // Compute the mean of the copied nodes.
            var centroid = Vector2.zero;
            var count = 1;
            foreach (var node in remappedNodes) {
                var position = node.DrawState.Position.position;
                centroid += (position - centroid) / count;
                ++count;
            }

            // Get the center of the current view
            var viewCenter = graphView.contentViewContainer.WorldToLocal(graphView.layout.center);

            foreach (var node in remappedNodes) {
                var drawState = node.DrawState;
                var positionRect = drawState.Position;
                var position = positionRect.position;
                position += viewCenter - centroid;
                positionRect.position = position;
                drawState.Position = positionRect;
                node.DrawState = drawState;
            }

            graphView.ClearSelection();
            graphView.DlogGraph.QueueSelection(remappedNodes, remappedEdges);
        }
    }
}