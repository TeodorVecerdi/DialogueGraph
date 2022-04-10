using System;
using DialogueGraph.Runtime;

namespace DialogueGraph {
    [Serializable]
    public class TriggerProperty : AbstractProperty {
        public TriggerProperty() {
            DisplayName = "Trigger";
            Type = PropertyType.Trigger;
        }

        public override AbstractProperty Copy() {
            return new TriggerProperty {
                DisplayName = DisplayName,
                Hidden = Hidden
            };
        }
    }
}