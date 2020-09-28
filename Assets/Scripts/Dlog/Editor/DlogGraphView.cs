using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Dlog {
    public class DlogGraphView : GraphView {
        private readonly DlogEditorWindow editorWindow;
        private Blackboard blackboard;

        public bool IsBlackboardVisible {
            get => blackboard.visible;
            set => blackboard.visible = value;
        }
        
        public DlogGraphView(DlogEditorWindow editorWindow) {
            this.editorWindow = editorWindow;
            this.AddStyleSheet("Graph");

            // Setup Graph
            SetupZoom(0.05f, 8f);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            blackboard = new Blackboard(this) {title = "Events and Properties", visible = false};
            Insert(1, blackboard);
        }

        
    }
}