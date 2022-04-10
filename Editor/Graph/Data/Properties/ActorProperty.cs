using System;
using DialogueGraph.Runtime;
using UnityEngine;

namespace DialogueGraph {
    [Serializable]
    public class ActorProperty : AbstractProperty {
        public ActorProperty() {
            DisplayName = "Actor";
            Type = PropertyType.Actor;
        }

        public override AbstractProperty Copy() {
            return new ActorProperty {
                DisplayName = DisplayName,
                Hidden = Hidden
            };
        }
    }

}