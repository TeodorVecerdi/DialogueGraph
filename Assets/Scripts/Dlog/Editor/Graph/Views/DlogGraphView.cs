using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public class DlogGraphView : GraphView {
        public static readonly Vector2 DefaultNodeSize = new Vector2(200, 150);
        public static readonly Rect DefaultNodePosition = new Rect(Vector2.zero, DefaultNodeSize);

        private readonly DlogEditorWindow editorWindow;
        private readonly DlogGraphObject dlogObject;
        private readonly BlackboardProvider blackboardProvider;
        private readonly EdgeConnectorListener edgeConnectorListener;

        public bool IsBlackboardVisible {
            get => blackboardProvider.Blackboard.visible;
            set => blackboardProvider.Blackboard.visible = value;
        }

        public DlogEditorWindow EditorWindow => editorWindow;
        public DlogGraphObject DlogObject => dlogObject;

        public DlogGraphView(DlogEditorWindow editorWindow, DlogGraphObject dlogObject) {
            this.dlogObject = dlogObject;
            this.editorWindow = editorWindow;
            this.AddStyleSheet("Graph");

            // Setup Graph
            SetupZoom(0.05f, 8f);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
            // TODO: Fix visibility issue with blackboard
            blackboardProvider = new BlackboardProvider(this);
            Insert(1, blackboardProvider.Blackboard);

            var searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            searchWindowProvider.Initialize(this.editorWindow, this);

            nodeCreationRequest = ctx => {
                searchWindowProvider.ConnectedPort = null;
                SearcherWindow.Show(editorWindow, searchWindowProvider.LoadSearchWindow(),
                    item => searchWindowProvider.OnSelectEntry(item, ctx.screenMousePosition - editorWindow.position.position),
                    ctx.screenMousePosition - editorWindow.position.position, null);
            };
            graphViewChanged += OnGraphViewChanged;
            edgeConnectorListener = new EdgeConnectorListener(this, searchWindowProvider);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            // var mousePosition = evt.mousePosition;
            base.BuildContextualMenu(evt);
            evt.menu.AppendSeparator();
            if (evt.target is Node || evt.target is StickyNote) {
                // TODO: GROUP
                evt.menu.AppendAction("Group Selection %g", _ => { }, actionStatusCallback => DropdownMenuAction.Status.Disabled);

                // TODO: UNGROUP
                evt.menu.AppendAction("Ungroup Selection %u", _ => { }, actionStatusCallback => DropdownMenuAction.Status.Disabled);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port => {
                if (startPort != port && startPort.node != port.node && port.direction != startPort.direction) {
                    // TODO: Add type check for ports
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            if (graphViewChange.movedElements != null) {
                editorWindow.GraphObject.RegisterCompleteObjectUndo("Moved elements");
                foreach (var node in graphViewChange.movedElements.OfType<TempNode>()) {
                    var rect = node.parent.ChangeCoordinatesTo(contentViewContainer, node.GetPosition());
                    node.Owner.DrawState.Position = rect;
                }
            }

            if (graphViewChange.edgesToCreate != null) {
                editorWindow.GraphObject.RegisterCompleteObjectUndo("Created edges");
                foreach (var edge in graphViewChange.edgesToCreate) {
                    dlogObject.DlogGraph.AddEdge(edge);
                }
                graphViewChange.edgesToCreate.Clear();
            }

            if (graphViewChange.elementsToRemove != null) {
                editorWindow.GraphObject.RegisterCompleteObjectUndo("Removed elements");
                foreach (var node in graphViewChange.elementsToRemove.OfType<TempNode>()) {
                    dlogObject.DlogGraph.RemoveNode(node.Owner);
                }

                foreach (var edge in graphViewChange.elementsToRemove.OfType<Edge>()) {
                    Debug.Log($"Removing edge, but is edge serialized edge? {edge.userData is SerializedEdge}");
                    dlogObject.DlogGraph.RemoveEdge((SerializedEdge)edge.userData);
                }

                /*var edgesToRemove = graphViewChange.elementsToRemove.OfType<Edge>().ToList();
                foreach (var edgeToRemove in edgesToRemove)
                    graphViewChange.elementsToRemove.Remove(edgeToRemove);*/
            }

            return graphViewChange;
        }

        private void OnKeyDown(KeyDownEvent evt) {
            if (evt.actionKey && evt.keyCode == KeyCode.G) {
                if (selection.OfType<GraphElement>().Any()) {
                    // TODO: GROUP
                }
            }

            if (evt.actionKey && evt.keyCode == KeyCode.U) {
                if (selection.OfType<GraphElement>().Any()) {
                    // TODO: UNGROUP
                }
            }
        }

        public void BuildGraph() {
            // Remove existing elements
            graphElements.ToList().OfType<Node>().ToList().ForEach(RemoveElement);
            graphElements.ToList().OfType<Edge>().ToList().ForEach(RemoveElement);
            graphElements.ToList().OfType<Group>().ToList().ForEach(RemoveElement);
            graphElements.ToList().OfType<StickyNote>().ToList().ForEach(RemoveElement);

            // Create & add graph elements 
            dlogObject.DlogGraph.Nodes.ForEach(AddNode);
            dlogObject.DlogGraph.Edges.ForEach(AddEdge);
        }

        public void HandleChanges() {
            foreach (var removedNode in dlogObject.DlogGraph.RemovedNodes) {
                RemoveNode(removedNode);
            }
            foreach (var removedEdge in dlogObject.DlogGraph.RemovedEdges) {
                RemoveEdge(removedEdge);
            }

            foreach (var addedNode in dlogObject.DlogGraph.AddedNodes) {
                AddNode(addedNode);
            }
            foreach (var addedEdge in dlogObject.DlogGraph.AddedEdges) {
                AddEdge(addedEdge);
            }
        }

        public void AddNode(SerializedNode nodeToAdd) {
            nodeToAdd.BuildNode(this, edgeConnectorListener);
            AddElement(nodeToAdd.Node);
        }

        public void RemoveNode(SerializedNode nodeToRemove) {
            if(nodeToRemove.Node != null)
                RemoveElement(nodeToRemove.Node);
            else {
                var view = GetNodeByGuid(nodeToRemove.GUID);
                if(view != null)
                    RemoveElement(view);
            }
        }

        public void AddEdge(SerializedEdge edgeToAdd) {
            edgeToAdd.BuildEdge(this);
            AddElement(edgeToAdd.Edge);
        }

        public void RemoveEdge(SerializedEdge edgeToRemove) {
            if (edgeToRemove.Edge != null) {
                RemoveElement(edgeToRemove.Edge);
            }
        }
    }
}