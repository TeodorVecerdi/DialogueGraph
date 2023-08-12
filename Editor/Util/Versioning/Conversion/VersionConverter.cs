using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DialogueGraph {
    internal static class VersionConverter {
        private static readonly SemVer s_V111 = (SemVer) "1.1.1";
        private static readonly SemVer s_V112 = (SemVer) "1.1.2";
        private static readonly SemVer s_V200 = (SemVer) "2.0.0";

        private static readonly SemVer[] s_SortedVersions = { s_V111, s_V112, s_V200 };

        private static bool s_BuiltMethodCache;
        private static Dictionary<SemVer, Func<JObject, JObject>> s_UpgradeMethodCache;

        private static SemVer GetNextVersion(SemVer from) {
            for (int i = 1; i < s_SortedVersions.Length; i++) {
                if (from >= s_SortedVersions[i - 1] && from < s_SortedVersions[i]) {
                    return s_SortedVersions[i];
                }
            }

            return SemVer.Invalid;
        }

        public static JObject Convert(SemVer from, SemVer to, JObject graphObject) {
            while (true) {
                if (from == to) return graphObject;
                SemVer next = GetNextVersion(from);
                if (next == SemVer.Invalid) {
                    Debug.Log($"Could not find upgrading method [{from} -> {to}]");
                    return graphObject;
                }

                graphObject = UpgradeTo(next, graphObject);
                from = next;
            }
        }

        [ConvertMethod("1.1.2")]
        private static JObject U_112(JObject graphObject) {
            graphObject["DialogueGraphVersion"] = s_V112.ToString();
            return graphObject;
        }

        private static readonly Regex u200TypeRegex = new(@"(?<quote>\\""|"")Dlog\.(.*?)(\k<quote>)", RegexOptions.Compiled);

        [ConvertMethod("2.0.0")]
        private static JObject U_200(JObject graphObject) {
            // Change namespaces
            string json = graphObject.ToString(Formatting.None);
            json = u200TypeRegex.Replace(json, "${quote}DialogueGraph.$1${quote}");

            graphObject = JObject.Parse(json);

            // Replace all CheckCombinerNode with either AndBooleanNode or OrBooleanNode
            JArray nodes = graphObject["nodes"] as JArray;
            foreach (JToken nodeToken in nodes!) {
                if (nodeToken.Value<string>("Type") != "DialogueGraph.CheckCombinerNode") continue;

                JObject nodeData = JObject.Parse(nodeToken.Value<string>("NodeData"));
                if (nodeData.Value<string>("operation") == "true") {
                    nodeToken["Type"] = "DialogueGraph.OrBooleanNode";
                } else {
                    nodeToken["Type"] = "DialogueGraph.AndBooleanNode";
                }
                nodeToken["NodeData"] = "{}";
            }

            graphObject["DialogueGraphVersion"] = s_V200.ToString();
            return graphObject;
        }

        private static JObject UpgradeTo(SemVer version, JObject graphObject) {
            if (!s_BuiltMethodCache) {
                BuildMethodCache();
            }
            if(s_UpgradeMethodCache.ContainsKey(version))
                return s_UpgradeMethodCache[version](graphObject);

            Debug.LogWarning($"Upgrade conversion with [target={version}] is not supported.");
            return graphObject;
        }


        private static void BuildMethodCache() {
            Debug.Log("Building cache");
            s_BuiltMethodCache = true;
            s_UpgradeMethodCache = new Dictionary<SemVer, Func<JObject, JObject>>();

            MethodInfo[] methods = typeof(VersionConverter).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo method in methods) {
                ConvertMethodAttribute attribute = method.GetCustomAttribute<ConvertMethodAttribute>(false);
                if (attribute == null) continue;

                Func<JObject, JObject> methodCall = method.CreateDelegate(typeof(Func<JObject, JObject>)) as Func<JObject, JObject>;
                s_UpgradeMethodCache.Add(attribute.TargetVersion, methodCall);
            }
        }
    }
}