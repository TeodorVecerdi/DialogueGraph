#nullable enable

using System;
using DialogueGraph.Runtime;

namespace DialogueGraph {
    internal static class EditorToRuntimeConversionExtensions {
        internal static Property ToRuntime(this AbstractProperty property)
            => new(property.GUID, property.ReferenceName, property.DisplayName, property.Type);

        internal static Edge ToRuntime(this SerializedEdge edge)
            => new(edge.Output, edge.OutputPort, edge.Input, edge.InputPort);

        internal static ConversationLine ToRuntime(this LineDataSelf line) => new() {
            Message = line.Line,
            Next = line.PortGuidA,
            TriggerPort = line.PortGuidB,
            CheckPort = Guid.Empty.ToString(),
        };

        internal static ConversationLine ToRuntime(this LineDataNpc line) => new() {
            Message = line.Line,
            Next = line.PortGuidA,
            TriggerPort = line.PortGuidB,
            CheckPort = line.PortGuidC,
        };
    }
}