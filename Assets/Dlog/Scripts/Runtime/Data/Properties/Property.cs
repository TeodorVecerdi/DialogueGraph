using System;

namespace Dlog.Runtime {
    [Serializable]
    public class Property {
        public string Guid;
        public string ReferenceName;
        public string DisplayName;
        public PropertyType Type;
    }
}