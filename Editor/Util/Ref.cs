using System;

namespace Dlog {
    public class Ref<T> : IEquatable<T>, IEquatable<Ref<T>> where T : struct {
        private T value;

        public Ref(T value) {
            this.value = value;
        }

        public Ref() {
            value = new T();
        }

        public ref T Get() {
            return ref value;
        }

        public T GetVal() {
            return value;
        }

        #region Equality Members
        public bool Equals(T other) {
            return value.Equals(other);
        }

        public bool Equals(Ref<T> other) {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other.value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Ref<T>) obj);
        }

        public override int GetHashCode() {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return value.GetHashCode();
        }
        
        public static bool operator ==(Ref<T> left, Ref<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(Ref<T> left, Ref<T> right) {
            return !Equals(left, right);
        }
        
        public static bool operator ==(Ref<T> left, T right) {
            if (ReferenceEquals(null, left))
                return false;
            return Equals(left.value, right);
        }

        public static bool operator !=(Ref<T> left, T right) {
            if (ReferenceEquals(null, left))
                return true;
            return !Equals(left.value, right);
        }
        #endregion

        public override string ToString() {
            return $"Ref<{typeof(T).Name}>[{value}]";
        }

        #region Operators
        public static implicit operator Ref<T>(T value) {
            return new Ref<T>(value);
        }
        #endregion
    }
}