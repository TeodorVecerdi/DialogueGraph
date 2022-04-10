using System;
using DialogueGraph.Runtime;

namespace DialogueGraph {
    [Serializable]
    public class CheckProperty : AbstractProperty {
        public CheckProperty() {
            DisplayName = "Check";
            Type = PropertyType.Check;
        }

        public override AbstractProperty Copy() {
            return new CheckProperty {
                DisplayName = DisplayName,
                Hidden = Hidden
            };
        }
    }
}