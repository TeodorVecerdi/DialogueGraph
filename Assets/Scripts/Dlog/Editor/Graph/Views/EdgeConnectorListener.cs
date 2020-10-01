using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;

namespace Dlog {
    public class EdgeConnectorListener : IEdgeConnectorListener {
        private DlogGraphView graphView;
        private SearchWindowProvider searchWindowProvider;

        public EdgeConnectorListener(DlogGraphView graphView, SearchWindowProvider searchWindowProvider) {
            this.graphView = graphView;
            this.searchWindowProvider = searchWindowProvider;
        }
        
        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            var port = edge.output?.edgeConnector.edgeDragHelper.draggedPort ?? edge.input?.edgeConnector.edgeDragHelper.draggedPort;
            searchWindowProvider.ConnectedPort = port;
            searchWindowProvider.RegenerateEntries = true;
            SearcherWindow.Show(graphView.EditorWindow, searchWindowProvider.LoadSearchWindow(), item => searchWindowProvider.OnSelectEntry(item, position), position, null);
            searchWindowProvider.RegenerateEntries = true;
        }

        public void OnDrop(GraphView graphView, Edge edge) {
            this.graphView.DlogObject.RegisterCompleteObjectUndo("Connect edge");
            this.graphView.DlogObject.DlogGraph.AddEdge(edge);
        }
    }
}