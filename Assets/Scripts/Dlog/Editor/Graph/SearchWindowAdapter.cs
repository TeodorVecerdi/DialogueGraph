using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public class SearchWindowAdapter : SearcherAdapter {
        private readonly VisualTreeAsset m_DefaultItemTemplate;
        public override bool HasDetailsPanel => false;

        public SearchWindowAdapter(string title) : base(title) {
            m_DefaultItemTemplate = Resources.Load<VisualTreeAsset>("SearcherItem");
        }
    }

    internal class SearchNodeItem : SearcherItem {
        public SearchWindowProvider.NodeEntry NodeEntry;
        public SearchNodeItem(string name, SearchWindowProvider.NodeEntry nodeEntry, string help = "", List<SearchNodeItem> children = null) : base(name) {
            NodeEntry = nodeEntry;
        }
    }
}