using System;
using UnityEngine;

namespace Dlog.Runtime {
    [Serializable]
    public class ActorData {
        public string Name;
        public ScriptableObject CustomData;
        public Property Property;

        public ActorData(string name, ScriptableObject customData, Property property) {
            Name = name;
            CustomData = customData;
            Property = property;
        }
    }
}