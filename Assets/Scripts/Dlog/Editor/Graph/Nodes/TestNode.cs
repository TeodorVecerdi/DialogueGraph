namespace Dlog {
    [Title("Test Node")]
    public class TestNode : TempNode {
        public TestNode() {
            Initialize("Test Node helo", DlogGraphView.DefaultNodePosition);
            Refresh();
        }
    }
}