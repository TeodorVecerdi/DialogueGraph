using System;

namespace DialogueGraph.Runtime {
    [Serializable]
    public class Edge {
        public string FromNode;
        public string FromPort;
        public string ToNode;
        public string ToPort;

        public Edge(string fromNode, string fromPort, string toNode, string toPort) {
            this.FromNode = fromNode;
            this.FromPort = fromPort;
            this.ToNode = toNode;
            this.ToPort = toPort;
        }
    }
}