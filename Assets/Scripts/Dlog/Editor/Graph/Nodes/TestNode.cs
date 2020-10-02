using UnityEditor.Experimental.GraphView;

namespace Dlog {
    [Title("Test Node")]
    public class TestNode : TempNode {
        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            Initialize("Test Node helo", EditorView.DefaultNodePosition);
            var input = TempPort.Create("Test Input", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object), edgeConnectorListener);
            var output = TempPort.Create("Test Output", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object), edgeConnectorListener);
            AddPort(input);
            AddPort(output);
            Refresh();
        }
    }
}