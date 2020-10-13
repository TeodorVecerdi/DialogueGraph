using System;
using UnityEngine;

namespace Dlog {
    [Serializable]
    public abstract class AbstractProperty {
        [SerializeField] public string GUID = $"{Convert.ToBase64String(Guid.NewGuid().ToByteArray()).GetHashCode():X}";
        [SerializeField] public PropertyType Type;
        [SerializeField] private string name;
        [SerializeField] private string defaultReferenceName;
        [SerializeField] private string overrideReferenceName;
        [SerializeField] private bool hidden;
        
        public string DisplayName {
            get {
                if (string.IsNullOrEmpty(name))
                    return $"{Type}_{GUID}";
                return name;
            }
            set => name = value;
        }

        public string ReferenceName {
            get {
                if (string.IsNullOrEmpty(OverrideReferenceName)) {
                    if (string.IsNullOrEmpty(defaultReferenceName))
                        defaultReferenceName = GetDefaultReferenceName();
                    return defaultReferenceName;
                }
                return OverrideReferenceName;
            }   
        }

        public string OverrideReferenceName {
            get => overrideReferenceName;
            set => overrideReferenceName = value;
        }

        public bool Hidden {
            get => hidden;
            set => hidden = value;
        }

        public virtual string GetDefaultReferenceName() {
            return $"{Type}_{GUID}";
        }

        public abstract AbstractProperty Copy();
    }

    [Serializable]
    public abstract class AbstractProperty<T> : AbstractProperty {
        private T value;
        public T Value {
            get => value;
            set => this.value = value;
        }
    }
}