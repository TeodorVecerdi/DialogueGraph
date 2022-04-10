using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Dlog {
    public class EditorView : VisualElement, IDisposable {
        public static readonly Vector2 DefaultNodeSize = new Vector2(200, 150);
        public static readonly Rect DefaultNodePosition = new Rect(Vector2.zero, DefaultNodeSize);

        private readonly DlogGraphView graphView;
        private readonly DlogEditorWindow editorWindow;
        private readonly DlogGraphObject dlogObject;
        private readonly BlackboardProvider blackboardProvider;
        private readonly EdgeConnectorListener edgeConnectorListener;
        private SearchWindowProvider searchWindowProvider;

        public bool IsBlackboardVisible {
            get => blackboardProvider.Blackboard.style.display == DisplayStyle.Flex;
            set => blackboardProvider.Blackboard.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public DlogEditorWindow EditorWindow => editorWindow;
        public DlogGraphObject DlogObject => dlogObject;
        public DlogGraphView GraphView => graphView;
        public EdgeConnectorListener EdgeConnectorListener => edgeConnectorListener;

        public EditorView(DlogEditorWindow editorWindow, DlogGraphObject dlogObject) {
            this.dlogObject = dlogObject;
            this.editorWindow = editorWindow;
            this.AddStyleSheet("Styles/Graph");
            
            
            var toolbar = new IMGUIContainer(() => {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button("Save Graph", EditorStyles.toolbarButton)) {
                    EditorWindow.Events.SaveRequested?.Invoke();
                }
                GUILayout.Space(6);
                if (GUILayout.Button("Save As...", EditorStyles.toolbarButton)) {
                    EditorWindow.Events.SaveAsRequested?.Invoke();
                }
                GUILayout.Space(6);
                if (GUILayout.Button("Show In Project", EditorStyles.toolbarButton)) {
                    EditorWindow.Events.ShowInProjectRequested?.Invoke();
                }

                GUILayout.FlexibleSpace();
                IsBlackboardVisible = GUILayout.Toggle(IsBlackboardVisible, "Blackboard", EditorStyles.toolbarButton);
                dlogObject.IsBlackboardVisible = IsBlackboardVisible;

                GUILayout.EndHorizontal();
            });
            
            Add(toolbar);
            var content = new VisualElement {name="content"};
            {
                graphView = new DlogGraphView(this);
                graphView.SetupZoom(0.05f, 8f);
                graphView.AddManipulator(new ContentDragger());
                graphView.AddManipulator(new SelectionDragger());
                graphView.AddManipulator(new RectangleSelector());
                graphView.AddManipulator(new ClickSelector());
                graphView.RegisterCallback<KeyDownEvent>(OnKeyDown);
                content.Add(graphView);

                var grid = new GridBackground();
                graphView.Insert(0, grid);
                grid.StretchToParentSize();

                blackboardProvider = new BlackboardProvider(this);
                graphView.Add(blackboardProvider.Blackboard);

                graphView.graphViewChanged += OnGraphViewChanged;
            }
            
            searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            searchWindowProvider.Initialize(this.editorWindow, this);

            graphView.nodeCreationRequest = ctx => {
                searchWindowProvider.ConnectedPort = null;
                SearcherWindow.Show(editorWindow, searchWindowProvider.LoadSearchWindow(),
                    item => searchWindowProvider.OnSelectEntry(item, ctx.screenMousePosition - editorWindow.position.position),
                    ctx.screenMousePosition - editorWindow.position.position, null);
            };
            edgeConnectorListener = new EdgeConnectorListener(this, searchWindowProvider);
            
            Add(content);
        }


        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            if (graphViewChange.movedElements != null) {
                editorWindow.GraphObject.RegisterCompleteObjectUndo("Moved elements");
                foreach (var node in graphViewChange.movedElements.OfType<AbstractNode>()) {
                    var rect = node.parent.ChangeCoordinatesTo(graphView.contentViewContainer, node.GetPosition());
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
                foreach (var node in graphViewChange.elementsToRemove.OfType<AbstractNode>()) {
                    dlogObject.DlogGraph.RemoveNode(node.Owner);
                }

                foreach (var edge in graphViewChange.elementsToRemove.OfType<Edge>()) {
                    dlogObject.DlogGraph.RemoveEdge((SerializedEdge)edge.userData);
                }

                foreach (var property in graphViewChange.elementsToRemove.OfType<BlackboardField>()) {
                    DlogObject.DlogGraph.RemoveProperty(property.userData as AbstractProperty);
                }
            }

            return graphViewChange;
        }

        private void OnKeyDown(KeyDownEvent evt) {
            if (evt.actionKey && evt.keyCode == KeyCode.G) {
                if (graphView.selection.OfType<GraphElement>().Any()) {
                    // TODO: GROUP
                }
            }

            if (evt.actionKey && evt.keyCode == KeyCode.U) {
                if (graphView.selection.OfType<GraphElement>().Any()) {
                    // TODO: UNGROUP
                }
            }
        }

        public void BuildGraph() {
            // Remove existing elements
            graphView.graphElements.ToList().OfType<Node>().ToList().ForEach(graphView.RemoveElement);
            graphView.graphElements.ToList().OfType<Edge>().ToList().ForEach(graphView.RemoveElement);
            graphView.graphElements.ToList().OfType<Group>().ToList().ForEach(graphView.RemoveElement);
            graphView.graphElements.ToList().OfType<StickyNote>().ToList().ForEach(graphView.RemoveElement);
            graphView.graphElements.ToList().OfType<BlackboardRow>().ToList().ForEach(graphView.RemoveElement);

            // Create & add graph elements
            dlogObject.DlogGraph.Nodes.ForEach(node => AddNode(node));
            dlogObject.DlogGraph.Edges.ForEach(AddEdge);
            dlogObject.DlogGraph.Properties.ForEach(AddProperty);
        }

        public void HandleChanges() {

            if(dlogObject.DlogGraph.AddedProperties.Any() || dlogObject.DlogGraph.RemovedProperties.Any())
                searchWindowProvider.RegenerateEntries = true;
            blackboardProvider.HandleChanges();
            
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
            
            foreach (var queuedNode in dlogObject.DlogGraph.NodeSelectionQueue) {
                graphView.AddToSelection(queuedNode.Node);
            }
            foreach (var queuedEdge in dlogObject.DlogGraph.EdgeSelectionQueue) {
                graphView.AddToSelection(queuedEdge.Edge);
            }
        }

        public void AddNode(SerializedNode nodeToAdd) {
            nodeToAdd.BuildNode(this, edgeConnectorListener);
            graphView.AddElement(nodeToAdd.Node);
        }

        public void RemoveNode(SerializedNode nodeToRemove) {
            if(nodeToRemove.Node != null)
                graphView.RemoveElement(nodeToRemove.Node);
            else {
                var view = graphView.GetNodeByGuid(nodeToRemove.GUID);
                if(view != null)
                    graphView.RemoveElement(view);
            }
        }

        public void AddEdge(SerializedEdge edgeToAdd) {
            edgeToAdd.BuildEdge(this);
            graphView.AddElement(edgeToAdd.Edge);
        }

        public void RemoveEdge(SerializedEdge edgeToRemove) {
            if (edgeToRemove.Edge != null) {
                edgeToRemove.Edge.input?.Disconnect(edgeToRemove.Edge);
                edgeToRemove.Edge.output?.Disconnect(edgeToRemove.Edge);
                graphView.RemoveElement(edgeToRemove.Edge);
            }
        }

        public void AddProperty(AbstractProperty property) {
            blackboardProvider.AddInputRow(property);
        }

        public void Dispose() {
            if (searchWindowProvider != null) {
                Object.DestroyImmediate(searchWindowProvider);
                searchWindowProvider = null;
            }
        }
    }
}