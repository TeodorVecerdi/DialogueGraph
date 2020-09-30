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
        private Blackboard blackboard;

        public bool IsBlackboardVisible {
            get => blackboard.visible;
            set => blackboard.visible = value;
        }

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

            blackboard = new Blackboard(this) {title = "Events and Properties", visible = false};
            Insert(1, blackboard);

            // var searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            var searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            searchWindowProvider.Initialize(this.editorWindow, this);

            // nodeCreationRequest = ctx => SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition), searchWindowProvider);
            nodeCreationRequest = ctx => {
                SearcherWindow.Show(editorWindow, searchWindowProvider.LoadSearchWindow(),
                    item => searchWindowProvider.OnSelectEntry(item, ctx.screenMousePosition - editorWindow.position.position),
                    ctx.screenMousePosition - editorWindow.position.position, null);
            };
            graphViewChanged += OnGraphViewChanged;
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

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            // Empty for now
            var action = "Graph changed";
            if (graphViewChange.movedElements?.Count > 0) action = "Moved elements";
            else if (graphViewChange.edgesToCreate?.Count > 0) action = "Created edges";
            else if (graphViewChange.elementsToRemove?.Count > 0) {
                action = "Removed elements";
            }

            editorWindow.GraphObject.RegisterCompleteObjectUndo(action);
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

        public void HandleChanges() {
            foreach (var addedNode in dlogObject.DlogGraph.AddedNodes) {
                if (addedNode.Node != null) {
                    AddNode(addedNode.Node);
                }
            }

            foreach (var removedNode in dlogObject.DlogGraph.RemovedNodes) {
                var view = (TempNode) GetNodeByGuid(removedNode.GUID);
                if (view != null)
                    RemoveNode(view);
            }
        }

        public void AddNode(TempNode nodeToAdd) {
            AddElement(nodeToAdd);
        }

        public void RemoveNode(TempNode nodeToRemove) {
            RemoveElement(nodeToRemove);
        }
    }
}