using System;

namespace Dlog.Runtime {
    [Serializable]
    public class Edge {
        public Guid FromNode;
        public Guid FromPort;
        public Guid ToNode;
        public Guid ToPort;
    }
}