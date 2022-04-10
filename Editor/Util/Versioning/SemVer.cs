using System;
using UnityEngine;

namespace DialogueGraph {
    [Serializable]
    public struct SemVer : IEquatable<SemVer>, IComparable<SemVer> {
        public static readonly SemVer Invalid = new SemVer {MAJOR = -1, MINOR = -1, PATCH = -1};

        // ReSharper disable once InconsistentNaming
        public int MAJOR;

        // ReSharper disable once InconsistentNaming
        public int MINOR;

        // ReSharper disable once InconsistentNaming
        public int PATCH;

        public SemVer(string versionString) {
            if (!IsValid(versionString, out var major, out var minor, out var patch)) {
                Debug.LogError($"Could not parse SemVer string {versionString} into format MAJOR.MINOR.PATCH.");
                this = Invalid;
                return;
            }

            MAJOR = major;
            MINOR = minor;
            PATCH = patch;
        }

        public SemVer(int major, int minor, int patch) {
            MAJOR = major;
            MINOR = minor;
            PATCH = patch;
        }

        public override string ToString() {
            return $"{MAJOR}.{MINOR}.{PATCH}";
        }

        public static implicit operator string(SemVer semVer) {
            return semVer.ToString();
        }

        public static explicit operator SemVer(string versionString) {
            return FromVersionString(versionString);
        }

        public static SemVer FromVersionString(string versionString) {
            return new SemVer(versionString);
        }

        public static bool IsValid(string versionString, out int major, out int minor, out int patch) {
            var split = versionString.Split('.');
            if (split.Length != 3) {
                major = -1;
                minor = -1;
                patch = -1;
                return false;
            }

            var majorWorks = int.TryParse(split[0], out var majorParsed) && majorParsed >= 0;
            var minorWorks = int.TryParse(split[1], out var minorParsed) && minorParsed >= 0;
            var patchWorks = int.TryParse(split[2], out var patchParsed) && patchParsed >= 0;
            major = majorParsed;
            minor = minorParsed;
            patch = patchParsed;
            return majorWorks && minorWorks && patchWorks;
        }

        public static bool IsValid(string versionString) => IsValid(versionString, out _, out _, out _);

        public bool Equals(SemVer other) {
            return MAJOR == other.MAJOR && MINOR == other.MINOR && PATCH == other.PATCH;
        }

        public override bool Equals(object obj) {
            return obj is SemVer other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = MAJOR;
                hashCode = (hashCode * 397) ^ MINOR;
                hashCode = (hashCode * 397) ^ PATCH;
                return hashCode;
            }
        }

        public static bool operator ==(SemVer left, SemVer right) {
            return left.Equals(right);
        }

        public static bool operator !=(SemVer left, SemVer right) {
            return !left.Equals(right);
        }

        public int CompareTo(SemVer other) {
            var majorComparison = MAJOR.CompareTo(other.MAJOR);
            if (majorComparison != 0)
                return majorComparison;
            var minorComparison = MINOR.CompareTo(other.MINOR);
            if (minorComparison != 0)
                return minorComparison;
            return PATCH.CompareTo(other.PATCH);
        }
    }
}