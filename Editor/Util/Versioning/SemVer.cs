using System;
using UnityEngine;

namespace Dlog {
    [Serializable]
    public struct SemVer {
        public static SemVer Invalid = new SemVer {MAJOR = -1, MINOR = -1, PATCH = -1};
        
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

        public void BumpMajor() {
            MAJOR++;
            MINOR = 0;
            PATCH = 0;
        }

        public void BumpMinor() {
            MINOR++;
            PATCH = 0;
        }

        public void BumpPatch() => PATCH++;

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
    }
}