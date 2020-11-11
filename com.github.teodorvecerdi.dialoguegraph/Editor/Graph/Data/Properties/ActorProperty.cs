using System;
using Dlog.Runtime;
using UnityEngine;

namespace Dlog {
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