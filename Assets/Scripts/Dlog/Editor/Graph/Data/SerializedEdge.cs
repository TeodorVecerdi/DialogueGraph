using System;

namespace Dlog {
    [Serializable]
    public class SerializedEdge {
        public string FromGUID;
        public string ToGUID;
        public int FromIndex;
        public int ToIndex;
    }
}