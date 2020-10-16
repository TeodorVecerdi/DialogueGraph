using System;
using UnityEngine;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace Dlog {
    [Serializable]
    public class SerializedEdge {
        [SerializeField] public string Input;
        [SerializeField] public string Output;
        [SerializeField] public string InputPort;
        [SerializeField] public string OutputPort;

        public Edge Edge;
        public EditorView EditorView;

        public void BuildEdge(EditorView editorView) {
            EditorView = editorView;
            var inputNode = editorView.GraphView.nodes.ToList().Find(node => node.viewDataKey == Input) as AbstractNode;
            var outputNode = editorView.GraphView.nodes.ToList().Find(node => node.viewDataKey == Output) as AbstractNode;
            var inputPort = inputNode.Owner.GuidPortDictionary[InputPort];
            var outputPort = outputNode.Owner.GuidPortDictionary[OutputPort];
            Edge = inputPort.ConnectTo(outputPort);
            Edge.userData = this;
        }
    }
}