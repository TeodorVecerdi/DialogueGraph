using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public class SearchWindowProvider : ScriptableObject {
        private DlogEditorWindow editorWindow;
        private EditorView editorView;
        private Texture2D icon;
        public List<NodeEntry> CurrentNodeEntries;
        public Port ConnectedPort;
        public bool RegenerateEntries { get; set; }

        public void Initialize(DlogEditorWindow editorWindow, EditorView editorView) {
            this.editorWindow = editorWindow;
            this.editorView = editorView;

            GenerateNodeEntries();
            icon = new Texture2D(1, 1);
            icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            icon.Apply();
        }

        private void OnDestroy() {
            if (icon != null) {
                DestroyImmediate(icon);
                icon = null;
            }
        }

        public void GenerateNodeEntries() {
            // First build up temporary data structure containing group & title as an array of strings (the last one is the actual title) and associated node type.
            var nodeEntries = new List<NodeEntry>();
            foreach (var type in TypeCache.GetTypesDerivedFrom<AbstractNode>()) {
                if (!type.IsClass || type.IsAbstract)
                    continue;

                if (type.GetCustomAttributes(typeof(TitleAttribute), false) is TitleAttribute[] attrs && attrs.Length > 0) {
                    foreach (var attr in attrs) {
                        AddEntries(type, attr.Title, nodeEntries);
                    }
                }
            }

            foreach (var property in editorView.DlogObject.DlogGraph.Properties) {
                var node = new SerializedNode(typeof(PropertyNode), new Rect(Vector2.zero, EditorView.DefaultNodeSize));
                node.BuildNode(editorView, editorView.EdgeConnectorListener, false);
                var propertyNode = node.Node as PropertyNode;
                propertyNode.PropertyGuid = property.GUID;
                node.BuildPortData();
                AddEntries(node, new[] {"Properties", $"{property.Type}: {property.DisplayName}"}, nodeEntries);
            }

            nodeEntries.Sort((entry1, entry2) => {
                for (var i = 0; i < entry1.Title.Length; i++) {
                    if (i >= entry2.Title.Length)
                        return 1;
                    var value = string.Compare(entry1.Title[i], entry2.Title[i], StringComparison.Ordinal);
                    if (value == 0)
                        continue;

                    // Make sure that leaves go before nodes
                    if (entry1.Title.Length != entry2.Title.Length && (i == entry1.Title.Length - 1 || i == entry2.Title.Length - 1)) {
                        //once nodes are sorted, sort slot entries by slot order instead of alphebetically
                        var alphaOrder = entry1.Title.Length < entry2.Title.Length ? -1 : 1;
                        var slotOrder = entry1.CompatiblePortIndex.CompareTo(entry2.CompatiblePortIndex);
                        return alphaOrder.CompareTo(slotOrder);
                    }

                    return value;
                }

                return 0;
            });

            CurrentNodeEntries = nodeEntries;
        }

        private void AddEntries(SerializedNode node, string[] title, List<NodeEntry> nodeEntries) {
            if (ConnectedPort == null) {
                nodeEntries.Add(new NodeEntry(node, title, -1, null));
                return;
            }

            var portIndices = new List<int>();
            for (var i = 0; i < node.Node.Ports.Count; i++) {
                if ((ConnectedPort as DlogPort).IsCompatibleWith(node.Node.Ports[i] as DlogPort) && ConnectedPort.direction != node.Node.Ports[i].direction) {
                    portIndices.Add(i);
                }
            }

            foreach (var portIndex in portIndices) {
                var newTitle = new string[title.Length];
                for (int i = 0; i < title.Length - 1; i++)
                    newTitle[i] = title[i];

                newTitle[title.Length - 1] = title[title.Length - 1];
                if (!string.IsNullOrEmpty(node.Node.Ports[portIndex].portName))
                    newTitle[title.Length - 1] += $" ({node.Node.Ports[portIndex].portName})";

                nodeEntries.Add(new NodeEntry(node, newTitle, portIndex, node.Node.Ports[portIndex].capacity));
            }
        }

        private void AddEntries(Type nodeType, string[] title, List<NodeEntry> nodeEntries) {
            if (ConnectedPort == null) {
                nodeEntries.Add(new NodeEntry(nodeType, title, -1, null));
                return;
            }

            var node = (AbstractNode) Activator.CreateInstance(nodeType);
            node.InitializeNode(null);
            var portIndices = new List<int>();
            for (var i = 0; i < node.Ports.Count; i++) {
                if ((ConnectedPort as DlogPort).IsCompatibleWith(node.Ports[i] as DlogPort) && ConnectedPort.direction != node.Ports[i].direction) {
                    portIndices.Add(i);
                }
            }

            foreach (var portIndex in portIndices) {
                var newTitle = new string[title.Length];
                for (int i = 0; i < title.Length - 1; i++)
                    newTitle[i] = title[i];
                newTitle[title.Length - 1] = title[title.Length - 1] + $" ({node.Ports[portIndex].portName})";

                nodeEntries.Add(new NodeEntry(nodeType, newTitle, portIndex, node.Ports[portIndex].capacity));
            }
        }

        public Searcher LoadSearchWindow() {
            if (RegenerateEntries) {
                GenerateNodeEntries();
                RegenerateEntries = false;
            }

            //create empty root for searcher tree
            var root = new List<SearcherItem>();
            var dummyEntry = new NodeEntry();

            foreach (var nodeEntry in CurrentNodeEntries) {
                SearcherItem parent = null;
                for (int i = 0; i < nodeEntry.Title.Length; i++) {
                    var pathEntry = nodeEntry.Title[i];
                    var children = parent != null ? parent.Children : root;
                    var item = children.Find(x => x.Name == pathEntry);

                    if (item == null) {
                        //if we don't have slot entries and are at a leaf, add userdata to the entry
                        if (i == nodeEntry.Title.Length - 1)
                            item = new SearchNodeItem(pathEntry, nodeEntry);

                        //if we aren't a leaf, don't add user data
                        else
                            item = new SearchNodeItem(pathEntry, dummyEntry);

                        if (parent != null) {
                            parent.AddChild(item);
                        } else {
                            children.Add(item);
                        }
                    }

                    parent = item;

                    if (parent.Depth == 0 && !root.Contains(parent))
                        root.Add(parent);
                }
            }

            var nodeDatabase = SearcherDatabase.Create(root, string.Empty, false);

            return new Searcher(nodeDatabase, new SearchWindowAdapter("Create Node"));
        }

        public bool OnSelectEntry(SearcherItem selectedEntry, Vector2 mousePosition) {
            if (selectedEntry == null || ((selectedEntry as SearchNodeItem).NodeEntry.Type == null && (selectedEntry as SearchNodeItem).NodeEntry.Node == null)) {
                return false;
            }

            var nodeEntry = (selectedEntry as SearchNodeItem).NodeEntry;
            var windowMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition);
            var graphMousePosition = editorView.GraphView.contentViewContainer.WorldToLocal(windowMousePosition);

            SerializedNode node;
            if (nodeEntry.Node != null) {
                node = nodeEntry.Node;
                node.DrawState.Position.position = graphMousePosition;
            } else {
                var nodeType = nodeEntry.Type;
                node = new SerializedNode(nodeType, new Rect(graphMousePosition, EditorView.DefaultNodeSize));
            }

            editorView.DlogObject.RegisterCompleteObjectUndo("Add " + node.Type);
            editorView.DlogObject.DlogGraph.AddNode(node);

            if (ConnectedPort != null) {
                if (nodeEntry.Node == null)
                    node.BuildNode(editorView, null);
                var edge = new SerializedEdge {
                    Output = ConnectedPort.node.viewDataKey,
                    Input = node.GUID,
                    OutputPort = ConnectedPort.viewDataKey,
                    InputPort = node.PortData[nodeEntry.CompatiblePortIndex],
                    OutputCapacity =  ConnectedPort.capacity,
                    InputCapacity = nodeEntry.Capacity.Value
                };
                editorView.DlogObject.DlogGraph.AddEdge(edge);
            }

            return true;
        }

        public struct NodeEntry : IEquatable<NodeEntry> {
            public readonly Type Type;
            public readonly string[] Title;
            public readonly int CompatiblePortIndex;
            public SerializedNode Node;
            public Port.Capacity? Capacity;

            public NodeEntry(Type type, string[] title, int compatiblePortIndex, Port.Capacity? capacity) {
                Type = type;
                Title = title;
                CompatiblePortIndex = compatiblePortIndex;
                Capacity = capacity;
                Node = null;
            }

            public NodeEntry(SerializedNode node, string[] title, int compatiblePortIndex, Port.Capacity? capacity) {
                Node = node;
                Title = title;
                CompatiblePortIndex = compatiblePortIndex;
                Type = Type.GetType(node.Type);
                Capacity = capacity;
            }

            public bool Equals(NodeEntry other) {
                return Equals(Title, other.Title) && Type == other.Type;
            }

            public override bool Equals(object obj) {
                return obj is NodeEntry other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    return ((Title != null ? Title.GetHashCode() : 0) * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                }
            }
        }
    }
}