using System;
using Dlog.Runtime;
using UnityEngine;

namespace Dlog {
    [Serializable]
    public abstract class AbstractProperty {
        [SerializeField] public string GUID = Guid.NewGuid().ToString();
        [SerializeField] public PropertyType Type;
        [SerializeField] private string name;
        [SerializeField] private string defaultReferenceName;
        [SerializeField] private string overrideReferenceName;
        [SerializeField] private bool hidden;
        
        public string DisplayName {
            get {
                if (string.IsNullOrEmpty(name))
                    return $"{Type}_{ShortGuid}";
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
            return $"{Type}_{ShortGuid}";
        }
        
        public string ShortGuid {
            get {
                if (string.IsNullOrEmpty(GUID))
                    GUID = Guid.NewGuid().ToString();
                if (Guid.TryParse(GUID, out var parsedGuid))
                    return $"{Convert.ToBase64String(parsedGuid.ToByteArray()).GetHashCode():X}";
                return $"{Convert.ToBase64String(Guid.NewGuid().ToByteArray()).GetHashCode():X}";
            }
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