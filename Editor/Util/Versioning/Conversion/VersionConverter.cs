using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DialogueGraph {
    public static class VersionConverter {
        private static readonly SemVer v111 = (SemVer) "1.1.1";
        private static readonly SemVer v112 = (SemVer) "1.1.2";
        private static readonly SemVer v200 = (SemVer) "2.0.0";

        private static readonly SemVer[] sortedVersions = {v111, v112, v200};

        private static bool builtMethodCache;
        private static Dictionary<SemVer, Func<JObject, JObject>> upgradeMethodCache;

        private static SemVer GetNextVersion(SemVer from) {
            for (int i = 1; i < sortedVersions.Length; i++) {
                int comparePrev = from.CompareTo(sortedVersions[i - 1]);
                int compareNext = from.CompareTo(sortedVersions[i]);
                if (comparePrev >= 0 && compareNext < 0) return sortedVersions[i];
            }

            return SemVer.Invalid;
        }

        public static JObject ConvertVersion(SemVer from, SemVer to, JObject jsonDlogObject) {
            while (true) {
                if (from == to) return jsonDlogObject;
                SemVer next = GetNextVersion(from);
                if (next == SemVer.Invalid) {
                    Debug.Log($"Could not find upgrading method [{from} -> {to}]");
                    return jsonDlogObject;
                }

                jsonDlogObject = UpgradeTo(next, jsonDlogObject);
                from = next;
            }
        }

        [ConvertMethod("1.1.2")]
        private static JObject U_112(JObject dlogObject) {
            dlogObject["DialogueGraphVersion"] = v112.ToString();
            return dlogObject;
        }

        private static readonly Regex u200TypeRegex = new Regex(@"(?<quote>\\""|"")Dlog\.(.*?)(\k<quote>)", RegexOptions.Compiled);
        [ConvertMethod("2.0.0")]
        private static JObject U_200(JObject dlogObject) {
            string json = dlogObject.ToString(Formatting.None);
            json = u200TypeRegex.Replace(json, "${quote}DialogueGraph.$1${quote}");

            dlogObject = JObject.Parse(json);
            dlogObject["DialogueGraphVersion"] = v200.ToString();
            return dlogObject;
        }

        private static JObject UpgradeTo(SemVer version, JObject dlogGraphObject) {
            if (!builtMethodCache) {
                BuildMethodCache();
            }
            if(upgradeMethodCache.ContainsKey(version))
                return upgradeMethodCache[version](dlogGraphObject);

            Debug.LogWarning($"Upgrade conversion with [target={version}] is not supported.");
            return dlogGraphObject;
        }


        private static void BuildMethodCache() {
            Debug.Log("Building cache");
            builtMethodCache = true;
            upgradeMethodCache = new Dictionary<SemVer, Func<JObject, JObject>>();

            MethodInfo[] methods = typeof(VersionConverter).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo method in methods) {
                ConvertMethodAttribute attribute = method.GetCustomAttribute<ConvertMethodAttribute>(false);
                if (attribute == null) continue;

                Func<JObject, JObject> methodCall = method.CreateDelegate(typeof(Func<JObject, JObject>)) as Func<JObject, JObject>;
                upgradeMethodCache.Add(attribute.TargetVersion, methodCall);
            }
        }
    }
}