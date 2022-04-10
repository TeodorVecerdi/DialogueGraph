using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;

namespace Dlog {
    public class EdgeConnectorListener : IEdgeConnectorListener {
        private EditorView editorView;
        private SearchWindowProvider searchWindowProvider;
        private GraphViewChange graphViewChange;
        private List<Edge> edgesToCreate;
        private List<GraphElement> edgesToDelete;

        public EdgeConnectorListener(EditorView editorView, SearchWindowProvider searchWindowProvider) {
            this.editorView = editorView;
            this.searchWindowProvider = searchWindowProvider;
            edgesToCreate = new List<Edge>();
            edgesToDelete = new List<GraphElement>();
            graphViewChange.edgesToCreate = edgesToCreate;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            var port = edge.output?.edgeConnector.edgeDragHelper.draggedPort ?? edge.input?.edgeConnector.edgeDragHelper.draggedPort;
            searchWindowProvider.ConnectedPort = port;
            searchWindowProvider.RegenerateEntries = true;
            SearcherWindow.Show(editorView.EditorWindow, searchWindowProvider.LoadSearchWindow(), item => searchWindowProvider.OnSelectEntry(item, position), position, null);
            searchWindowProvider.RegenerateEntries = true;
        }

        public void OnDrop(GraphView graphView, Edge edge) {
            if(editorView.DlogObject.DlogGraph.HasEdge(edge)) return;
            editorView.DlogObject.RegisterCompleteObjectUndo("Connect edge");
            editorView.DlogObject.DlogGraph.AddEdge(edge);
        }
    }
}