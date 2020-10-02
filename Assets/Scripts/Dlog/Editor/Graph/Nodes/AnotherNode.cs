using UnityEditor.Experimental.GraphView;

namespace Dlog {
    [Title("Group", "Another Node")]
    public class AnotherNode : TempNode {
        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            Initialize("I am Node bonjour", EditorView.DefaultNodePosition);
            var input = TempPort.Create("Test Input", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object), edgeConnectorListener);
            var input2 = TempPort.Create("Test Input 2", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object), edgeConnectorListener);
            AddPort(input);
            AddPort(input2);
            Refresh();
        }
    }
}