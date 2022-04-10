using System;

namespace Dlog {
    public class Ref<T> : IEquatable<T>, IEquatable<Ref<T>> where T : struct {
        private T value;
        private Func<T> getValue = null;
        private Action setValue = null;

        private Ref(T value) {
            this.value = value;
        }

        private Ref() {
            value = new T();
        }

        public ref T GetReference() {
            if (getValue != null) {
                var val = getValue();
                if (!val.Equals(value)) value = val;
            }
            return ref value;
        }

        public T GetValue() {
            if (getValue != null) {
                var val = getValue();
                if (!val.Equals(value)) value = val;
            }
            return value;
        }

        public void Set(T newValue) {
            value = newValue;
            setValue?.Invoke();
        }

        public void SetValueUnbound(T newValue) {
            value = newValue;
        }

        public T GetValueUnbound() {
            return value;
        }

        public void Bind(Func<T> getValue, Action setValue) {
            this.getValue = getValue;
            this.setValue = setValue;
        }

        public void Unbind() {
            getValue = null;
            setValue = null;
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

        public static Ref<T> MakeRef(T initialValue, Func<T> getValue, Action setValue) {
            var reference = new Ref<T>(initialValue);
            reference.Bind(getValue, setValue);
            return reference;
        }

        public static Ref<T> MakeRef(T initialValue) {
            return new Ref<T>(initialValue);
        }

        public static Ref<T> MakeRef() {
            return new Ref<T>();
        }

        #region Operators
        public static explicit operator Ref<T>(T value) {
            return new Ref<T>(value);
        }
        #endregion
    }
}