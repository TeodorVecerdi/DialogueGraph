using System;
using UnityEngine;

namespace Dlog {
    public struct SemVer {
        // ReSharper disable once InconsistentNaming
        public int MAJOR;

        // ReSharper disable once InconsistentNaming
        public int MINOR;

        // ReSharper disable once InconsistentNaming
        public int PATCH;

        public SemVer(string versionString) {
            if (!IsValid(versionString)) {
                Debug.LogError($"Could not parse SemVer string {versionString} into format MAJOR.MINOR.PATCH.");
                this = Invalid;
                return;
            }
            
            var split = versionString.Split('.');
            MAJOR = int.Parse(split[0]);
            MINOR = int.Parse(split[1]);
            PATCH = int.Parse(split[2]);
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

        public static SemVer Invalid = new SemVer {MAJOR = -1, MINOR = -1, PATCH = -1};

        public static SemVer FromVersionString(string versionString) {
            return new SemVer(versionString);
        }

        public static bool IsValid(string versionString) {
            var split = versionString.Split('.');
            if (split.Length != 3) {
                return false;
            }

            var majorWorks = int.TryParse(split[0], out var _1);
            var minorWorks = int.TryParse(split[1], out var _2);
            var patchWorks = int.TryParse(split[2], out var _3);
            return majorWorks && minorWorks && patchWorks;
        }
    }
}