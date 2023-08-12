using System;

namespace DialogueGraph.Runtime {
    [Serializable]
    public class Property {
        public string Guid;
        public string ReferenceName;
        public string DisplayName;
        public PropertyType Type;

        public Property(string guid, string referenceName, string displayName, PropertyType type) {
            this.Guid = guid;
            this.ReferenceName = referenceName;
            this.DisplayName = displayName;
            this.Type = type;
        }
    }
}