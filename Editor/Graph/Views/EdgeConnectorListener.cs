using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;

namespace DialogueGraph {
    public class EdgeConnectorListener : IEdgeConnectorListener {
        private readonly EditorView m_EditorView;
        private readonly SearchWindowProvider m_SearchWindowProvider;

        public EdgeConnectorListener(EditorView editorView, SearchWindowProvider searchWindowProvider) {
            this.m_EditorView = editorView;
            this.m_SearchWindowProvider = searchWindowProvider;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            Port port = edge.output?.edgeConnector.edgeDragHelper.draggedPort ?? edge.input?.edgeConnector.edgeDragHelper.draggedPort;
            this.m_SearchWindowProvider.ConnectedPort = port;
            this.m_SearchWindowProvider.RegenerateEntries = true;
            SearcherWindow.Show(this.m_EditorView.EditorWindow, this.m_SearchWindowProvider.LoadSearchWindow(), item => this.m_SearchWindowProvider.OnSelectEntry(item, position), position, null);
            this.m_SearchWindowProvider.RegenerateEntries = true;
        }

        public void OnDrop(GraphView graphView, Edge edge) {
            DlogGraphObject graphObject = this.m_EditorView.DlogObject;

            if (graphObject.GraphData.HasEdge(edge)) return;
            graphObject.RegisterCompleteObjectUndo("Connect edge");
            graphObject.GraphData.AddEdge(edge);
        }
    }
}