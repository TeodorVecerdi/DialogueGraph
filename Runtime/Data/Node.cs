using System;
using System.Collections.Generic;

namespace DialogueGraph.Runtime {
    [Serializable]
    public class Node {
        public NodeType Type;
        public string Guid;

        public string ActorGuid;

        /// <summary>
        /// Since multiple nodes can be a previous node to a certain node, we are only
        /// using this property to find out where the graph begins. We don't care if a
        /// node has multiple previous nodes because you never need to backtrack in the
        /// graph. You will only walk forwards in the graph.
        /// </summary>
        public string Previous;
        public List<ConversationLine> Lines;
        public string Temp_PropertyNodeGuid;
    }

    public enum NodeType {
        NPC,
        SELF,
        PROP,
        COMBINER,
        
        BOOLEAN_NOT,
        BOOLEAN_AND,
        BOOLEAN_OR,
        BOOLEAN_XOR,
        BOOLEAN_NAND,
        BOOLEAN_NOR,
        BOOLEAN_XNOR,
    }
}