using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Dlog {
    public static class VersionConverter {
        private static readonly SemVer v111 = (SemVer) "1.1.1";
        private static readonly SemVer v112 = (SemVer) "1.1.2";

        private static SemVer[] sortedVersions = {v111, v112};

        private static bool builtMethodCache;
        private static Dictionary<SemVer, Action<DlogGraphObject>> upgradeMethodCache;

        private static SemVer GetNextVersion(SemVer from) {
            for (var i = 1; i < sortedVersions.Length; i++) {
                var comparePrev = from.CompareTo(sortedVersions[i - 1]);
                var compareNext = from.CompareTo(sortedVersions[i]);
                if (comparePrev >= 0 && compareNext < 0) return sortedVersions[i];
            }

            return SemVer.Invalid;
        }

        public static void ConvertVersion(SemVer from, SemVer to, DlogGraphObject dlogObject) {
            if (from == to) return;
            var next = GetNextVersion(from);
            if (next == SemVer.Invalid) {
                Debug.Log($"Could not find upgrading method [{from} -> {to}]");
                return;
            }

            UpgradeTo(next, dlogObject);
            ConvertVersion(next, to, dlogObject);
        }


        [ConvertMethod("1.1.2")]
        private static void U_112(DlogGraphObject dlogObject) {
            dlogObject.DlogGraph.DialogueGraphVersion = v112;
        }

        private static void UpgradeTo(SemVer version, DlogGraphObject dlogGraphObject) {
            if (!builtMethodCache) {
                BuildMethodCache();
            }
            if(upgradeMethodCache.ContainsKey(version))
                upgradeMethodCache[version](dlogGraphObject);
            else Debug.LogWarning($"Upgrade conversion with [target={version}] is not supported.");
        }


        private static void BuildMethodCache() {
            Debug.Log("Building cache");
            builtMethodCache = true;
            upgradeMethodCache = new Dictionary<SemVer, Action<DlogGraphObject>>();

            var methods = typeof(VersionConverter).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods) {
                var attributes = method.GetCustomAttributes<ConvertMethodAttribute>(false).ToList();
                if (attributes.Count <= 0) continue;
                var attribute = attributes[0];

                var methodCall = method.CreateDelegate(typeof(Action<DlogGraphObject>)) as Action<DlogGraphObject>;
                upgradeMethodCache.Add(attribute.TargetVersion, methodCall);
            }
        }
    }
}