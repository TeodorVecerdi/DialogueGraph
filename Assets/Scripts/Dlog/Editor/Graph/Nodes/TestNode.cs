using UnityEditor.Experimental.GraphView;

namespace Dlog {
    [Title("Test Node")]
    public class TestNode : AbstractNode {
        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            Initialize("Test Node helo", EditorView.DefaultNodePosition);
            var input = DlogPort.Create("Test Input", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object), edgeConnectorListener);
            var output = DlogPort.Create("Test Output", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object), edgeConnectorListener);
            AddPort(input);
            AddPort(output);
            Refresh();
        }
    }
}