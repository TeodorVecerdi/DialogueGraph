using System;
using UnityEngine;

namespace Dlog {
    [Serializable]
    public class ActorProperty : AbstractProperty<ActorData> {
        public ActorProperty() {
            DisplayName = "Actor";
            Type = PropertyType.Actor;
            Value = new ActorData {Name = "Unnamed Actor"};
        }

        public override AbstractProperty Copy() {
            return new ActorProperty {
                DisplayName = DisplayName,
                Hidden = Hidden,
                Value = Value
            };
        }
    }

    [Serializable]
    public class ActorData {
        [SerializeField] public string Name;
    }
}