using System;

namespace Dlog.Runtime {
    [Serializable]
    public class Property {
        public Guid Guid;
        public string ReferenceName;
        public string DisplayName;
        public PropertyType Type;
    }
}