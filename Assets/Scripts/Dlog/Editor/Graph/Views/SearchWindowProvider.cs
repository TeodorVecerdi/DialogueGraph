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
        private DlogGraphView graphView;
        private Texture2D icon;
        public List<NodeEntry> CurrentNodeEntries;
        public Port ConnectedPort;
        public bool RegenerateEntries { get; set; }

        public void Initialize(DlogEditorWindow editorWindow, DlogGraphView graphView) {
            this.editorWindow = editorWindow;
            this.graphView = graphView;

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
            foreach (var type in TypeCache.GetTypesDerivedFrom<TempNode>()) {
                if (!type.IsClass || type.IsAbstract)
                    continue;

                if (type.GetCustomAttributes(typeof(TitleAttribute), false) is TitleAttribute[] attrs && attrs.Length > 0) {
                    AddEntries(type, attrs[0].Title, nodeEntries);
                }
            }

            nodeEntries.Sort((entry1, entry2) => {
                for (var i = 0; i < entry1.Title.Length; i++) {
                    if (i >= entry2.Title.Length)
                        return 1;
                    var value = string.Compare(entry1.Title[i], entry2.Title[i], StringComparison.Ordinal);
                    if (value != 0) {
                        // Make sure that leaves go before nodes
                        if (entry1.Title.Length != entry2.Title.Length && (i == entry1.Title.Length - 1 || i == entry2.Title.Length - 1))
                            return entry1.Title.Length < entry2.Title.Length ? -1 : 1;
                        return value;
                    }
                }

                return 0;
            });

            CurrentNodeEntries = nodeEntries;
        }

        private void AddEntries(Type nodeType, string[] title, List<NodeEntry> nodeEntries) {
            if (ConnectedPort == null) {
                nodeEntries.Add(new NodeEntry(nodeType, title, -1));
                return;
            }

            var node = (TempNode)Activator.CreateInstance(nodeType);
            node.InitializeNode(null);
            var portIndices = new List<int>();
            for(var i = 0; i < node.Ports.Count; i++) portIndices.Add(i);
            portIndices.RemoveAll(portIndex => ConnectedPort.portType != node.Ports[portIndex].portType || ConnectedPort.direction == node.Ports[portIndex].direction);
            foreach (var portIndex in portIndices) {
                var newTitle = new string[title.Length];
                for (int i = 0; i < title.Length - 1; i++)
                    newTitle[i] = title[i];
                newTitle[title.Length-1] = title[title.Length - 1] + $" ({node.Ports[portIndex].portName})";
                
                nodeEntries.Add(new NodeEntry(nodeType, newTitle, portIndex));
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
            if (selectedEntry == null || (selectedEntry as SearchNodeItem).NodeEntry.Type == null) {
                return false;
            }

            var nodeEntry = (selectedEntry as SearchNodeItem).NodeEntry;
            var nodeType = nodeEntry.Type;
            
            var windowMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);
            var node = new SerializedNode(nodeType, new Rect(graphMousePosition, DlogGraphView.DefaultNodeSize));

            graphView.DlogObject.RegisterCompleteObjectUndo("Add " + node.Type);
            graphView.DlogObject.DlogGraph.AddNode(node);

            if (ConnectedPort != null) {
                node.BuildNode(graphView, null);
                var edge = new SerializedEdge {
                    Output = ConnectedPort.node.viewDataKey,
                    Input = node.GUID,
                    OutputPort = ConnectedPort.viewDataKey,
                    InputPort = node.PortData[nodeEntry.CompatiblePortIndex]
                };
                graphView.DlogObject.DlogGraph.AddEdge(edge);
            }
            
            return true;
        }

        public readonly struct NodeEntry : IEquatable<NodeEntry> {
            public readonly Type Type;
            public readonly string[] Title;
            public readonly int CompatiblePortIndex;

            public NodeEntry(Type type, string[] title, int compatiblePortIndex) {
                Type = type;
                Title = title;
                CompatiblePortIndex = compatiblePortIndex;
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