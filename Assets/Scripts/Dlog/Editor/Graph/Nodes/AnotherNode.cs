namespace Dlog {
    [Title("Group", "Another Node")]
    public class AnotherNode : TempNode {
        public AnotherNode() {
            Initialize("I am Node bonjour", DlogGraphView.DefaultNodePosition);
            Refresh();
        }
    }
}