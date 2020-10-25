using System;

namespace Dlog.Runtime {
    // Node currentNode, int conversationLineIndex, bool returnValue;
    [Serializable] public class CheckEvent : SerializableCallback<Node, int, bool> {}
    // Guid currentNode
    [Serializable] public class TriggerEvent : SerializableEvent<Node, int> {}
}