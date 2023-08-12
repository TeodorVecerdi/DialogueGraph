using UnityEditor.Searcher;

namespace DialogueGraph {
    public class SearchWindowAdapter : SearcherAdapter {
        public override bool HasDetailsPanel => false;

        public SearchWindowAdapter(string title) : base(title) {
        }
    }

    internal class SearchNodeItem : SearcherItem {
        public readonly SearchWindowProvider.NodeEntry NodeEntry;
        public SearchNodeItem(string name, SearchWindowProvider.NodeEntry nodeEntry) : base(name) {
            this.NodeEntry = nodeEntry;
        }
    }
}