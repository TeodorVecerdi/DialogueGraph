using System;
using System.Collections.Generic;

namespace Dlog.Runtime {
    [Serializable]
    public class Node {
        public NodeType Type;
        public Guid Guid;

        public Guid ActorGuid;
        
        /// <summary>
        /// Since multiple nodes can be a previous node to a certain node, we are only
        /// using this property to find out where the graph begins. We don't care if a
        /// node has multiple previous nodes because you never need to backtrack in the
        /// graph. You will only walk forwards in the graph.
        /// </summary>
        public Guid Previous;
        public List<ConversationLine> Lines;
        public Guid Temp_PropertyNodeGuid;
    }

    public enum NodeType {
        NPC,
        SELF,
        PROP
    }
}