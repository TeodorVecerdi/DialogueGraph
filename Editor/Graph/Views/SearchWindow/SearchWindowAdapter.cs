using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueGraph {
    public class SearchWindowAdapter : SearcherAdapter {
        public override bool HasDetailsPanel => false;

        public SearchWindowAdapter(string title) : base(title) {
        }
    }

    internal class SearchNodeItem : SearcherItem {
        public SearchWindowProvider.NodeEntry NodeEntry;
        public SearchNodeItem(string name, SearchWindowProvider.NodeEntry nodeEntry, string help = "", List<SearchNodeItem> children = null) : base(name) {
            NodeEntry = nodeEntry;
        }
    }
}