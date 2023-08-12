using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DialogueGraph {
    public class EditorView : VisualElement, IDisposable {
        public static readonly Vector2 DefaultNodeSize = new Vector2(200, 150);
        public static readonly Rect DefaultNodePosition = new Rect(Vector2.zero, DefaultNodeSize);

        private readonly BlackboardProvider blackboardProvider;
        private SearchWindowProvider searchWindowProvider;

        public bool IsBlackboardVisible {
            get => blackboardProvider.Blackboard.style.display == DisplayStyle.Flex;
            set => blackboardProvider.Blackboard.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public DlogEditorWindow EditorWindow { get; }

        public DlogGraphObject DlogObject => EditorWindow.GraphObject;

        public DlogGraphView GraphView { get; }

        public EdgeConnectorListener EdgeConnectorListener { get; }

        public EditorView(DlogEditorWindow editorWindow) {
            EditorWindow = editorWindow;
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
                editorWindow.GraphObject.IsBlackboardVisible = IsBlackboardVisible;

                GUILayout.EndHorizontal();
            });
            
            Add(toolbar);
            var content = new VisualElement {name="content"};
            {
                GraphView = new DlogGraphView(this);
                GraphView.SetupZoom(0.05f, 8f);
                GraphView.AddManipulator(new ContentDragger());
                GraphView.AddManipulator(new SelectionDragger());
                GraphView.AddManipulator(new RectangleSelector());
                GraphView.AddManipulator(new ClickSelector());
                GraphView.RegisterCallback<KeyDownEvent>(OnKeyDown);
                content.Add(GraphView);

                var grid = new GridBackground();
                GraphView.Insert(0, grid);
                grid.StretchToParentSize();

                blackboardProvider = new BlackboardProvider(this);
                GraphView.Add(blackboardProvider.Blackboard);

                GraphView.graphViewChanged += OnGraphViewChanged;
            }
            
            searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            searchWindowProvider.Initialize(EditorWindow, this);

            GraphView.nodeCreationRequest = ctx => {
                searchWindowProvider.ConnectedPort = null;
                SearcherWindow.Show(editorWindow, searchWindowProvider.LoadSearchWindow(),
                    item => searchWindowProvider.OnSelectEntry(item, ctx.screenMousePosition - editorWindow.position.position),
                    ctx.screenMousePosition - editorWindow.position.position, null);
            };
            EdgeConnectorListener = new EdgeConnectorListener(this, searchWindowProvider);
            
            Add(content);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            if (graphViewChange.movedElements != null) {
                EditorWindow.GraphObject.RegisterCompleteObjectUndo("Moved elements");
                foreach (var node in graphViewChange.movedElements.OfType<AbstractNode>()) {
                    var rect = node.parent.ChangeCoordinatesTo(GraphView.contentViewContainer, node.GetPosition());
                    node.Owner.DrawState.Position = rect;
                }
            }

            if (graphViewChange.edgesToCreate != null) {
                EditorWindow.GraphObject.RegisterCompleteObjectUndo("Created edges");
                foreach (var edge in graphViewChange.edgesToCreate) {
                    DlogObject.GraphData.AddEdge(edge);
                }
                graphViewChange.edgesToCreate.Clear();
            }

            if (graphViewChange.elementsToRemove != null) {
                EditorWindow.GraphObject.RegisterCompleteObjectUndo("Removed elements");
                foreach (var node in graphViewChange.elementsToRemove.OfType<AbstractNode>()) {
                    DlogObject.GraphData.RemoveNode(node.Owner);
                }

                foreach (var edge in graphViewChange.elementsToRemove.OfType<Edge>()) {
                    DlogObject.GraphData.RemoveEdge((SerializedEdge)edge.userData);
                }

                foreach (var property in graphViewChange.elementsToRemove.OfType<BlackboardField>()) {
                    DlogObject.GraphData.RemoveProperty(property.userData as AbstractProperty);
                }
            }

            return graphViewChange;
        }

        private void OnKeyDown(KeyDownEvent evt) {
            if (evt.actionKey && evt.keyCode == KeyCode.G) {
                if (GraphView.selection.OfType<GraphElement>().Any()) {
                    // TODO: GROUP
                }
            }

            if (evt.actionKey && evt.keyCode == KeyCode.U) {
                if (GraphView.selection.OfType<GraphElement>().Any()) {
                    // TODO: UNGROUP
                }
            }
        }

        public void BuildGraph() {
            // Remove existing elements
            GraphView.graphElements.ToList().OfType<Node>().ToList().ForEach(GraphView.RemoveElement);
            GraphView.graphElements.ToList().OfType<Edge>().ToList().ForEach(GraphView.RemoveElement);
            GraphView.graphElements.ToList().OfType<Group>().ToList().ForEach(GraphView.RemoveElement);
            GraphView.graphElements.ToList().OfType<StickyNote>().ToList().ForEach(GraphView.RemoveElement);
            GraphView.graphElements.ToList().OfType<BlackboardRow>().ToList().ForEach(GraphView.RemoveElement);

            // Create & add graph elements
            DlogObject.GraphData.Nodes.ForEach(node => AddNode(node));
            DlogObject.GraphData.Edges.ForEach(AddEdge);
            DlogObject.GraphData.Properties.ForEach(AddProperty);
        }

        public void HandleChanges() {

            if(DlogObject.GraphData.AddedProperties.Any() || DlogObject.GraphData.RemovedProperties.Any())
                searchWindowProvider.RegenerateEntries = true;
            blackboardProvider.HandleChanges();
            
            foreach (var removedNode in DlogObject.GraphData.RemovedNodes) {
                RemoveNode(removedNode);
            }
            foreach (var removedEdge in DlogObject.GraphData.RemovedEdges) {
                RemoveEdge(removedEdge);
            }

            foreach (var addedNode in DlogObject.GraphData.AddedNodes) {
                AddNode(addedNode);
            }
            foreach (var addedEdge in DlogObject.GraphData.AddedEdges) {
                AddEdge(addedEdge);
            }
            
            foreach (var queuedNode in DlogObject.GraphData.NodeSelectionQueue) {
                GraphView.AddToSelection(queuedNode.Node);
            }
            foreach (var queuedEdge in DlogObject.GraphData.EdgeSelectionQueue) {
                GraphView.AddToSelection(queuedEdge.Edge);
            }
        }

        public void AddNode(SerializedNode nodeToAdd) {
            nodeToAdd.BuildNode(this, EdgeConnectorListener);
            GraphView.AddElement(nodeToAdd.Node);
        }

        public void RemoveNode(SerializedNode nodeToRemove) {
            if(nodeToRemove.Node != null)
                GraphView.RemoveElement(nodeToRemove.Node);
            else {
                var view = GraphView.GetNodeByGuid(nodeToRemove.GUID);
                if(view != null)
                    GraphView.RemoveElement(view);
            }
        }

        public void AddEdge(SerializedEdge edgeToAdd) {
            edgeToAdd.BuildEdge(this);
            GraphView.AddElement(edgeToAdd.Edge);
        }

        public void RemoveEdge(SerializedEdge edgeToRemove) {
            if (edgeToRemove.Edge != null) {
                edgeToRemove.Edge.input?.Disconnect(edgeToRemove.Edge);
                edgeToRemove.Edge.output?.Disconnect(edgeToRemove.Edge);
                GraphView.RemoveElement(edgeToRemove.Edge);
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