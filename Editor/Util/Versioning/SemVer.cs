using System;
using UnityEngine;

namespace DialogueGraph {
    [Serializable]
    public struct SemVer : IEquatable<SemVer>, IComparable<SemVer> {
        public static readonly SemVer Invalid = new() { MAJOR = -1, MINOR = -1, PATCH = -1 };

        // ReSharper disable once InconsistentNaming
        public int MAJOR;

        // ReSharper disable once InconsistentNaming
        public int MINOR;

        // ReSharper disable once InconsistentNaming
        public int PATCH;

        public SemVer(string versionString) {
            if (!IsValid(versionString, out int major, out int minor, out int patch)) {
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

        public override string ToString() => $"{MAJOR}.{MINOR}.{PATCH}";

        public static SemVer FromVersionString(string versionString) => new(versionString);

        public static bool IsValid(string versionString, out int major, out int minor, out int patch) {
            string[] split = versionString.Split('.');
            if (split.Length != 3) {
                major = -1;
                minor = -1;
                patch = -1;
                return false;
            }

            bool majorWorks = int.TryParse(split[0], out int majorParsed) && majorParsed >= 0;
            bool minorWorks = int.TryParse(split[1], out int minorParsed) && minorParsed >= 0;
            bool patchWorks = int.TryParse(split[2], out int patchParsed) && patchParsed >= 0;
            major = majorParsed;
            minor = minorParsed;
            patch = patchParsed;
            return majorWorks && minorWorks && patchWorks;
        }

        public static bool IsValid(string versionString) => IsValid(versionString, out _, out _, out _);

        public static implicit operator string(SemVer semVer) => semVer.ToString();
        public static explicit operator SemVer(string versionString) => FromVersionString(versionString);

        public bool Equals(SemVer other) => MAJOR == other.MAJOR && MINOR == other.MINOR && PATCH == other.PATCH;
        public override bool Equals(object obj) => obj is SemVer other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(MAJOR, MINOR, PATCH);

        public int CompareTo(SemVer other) {
            int majorComparison = MAJOR.CompareTo(other.MAJOR);
            if (majorComparison != 0) return majorComparison;

            int minorComparison = MINOR.CompareTo(other.MINOR);
            if (minorComparison != 0) return minorComparison;

            return PATCH.CompareTo(other.PATCH);
        }

        public static bool operator ==(SemVer left, SemVer right) => left.Equals(right);
        public static bool operator !=(SemVer left, SemVer right) => !left.Equals(right);
        public static bool operator <(SemVer left, SemVer right) => left.CompareTo(right) < 0;
        public static bool operator >(SemVer left, SemVer right) => left.CompareTo(right) > 0;
        public static bool operator <=(SemVer left, SemVer right) => left.CompareTo(right) <= 0;
        public static bool operator >=(SemVer left, SemVer right) => left.CompareTo(right) >= 0;
    }
}