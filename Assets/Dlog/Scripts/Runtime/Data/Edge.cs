using System;

namespace Dlog.Runtime {
    [Serializable]
    public class Edge {
        public string FromNode;
        public string FromPort;
        public string ToNode;
        public string ToPort;
    }
}