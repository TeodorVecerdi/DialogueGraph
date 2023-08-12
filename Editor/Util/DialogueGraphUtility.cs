using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DialogueGraph {
    public static class DialogueGraphUtility {
        internal static readonly SemVer LatestVersion = new SemVer(2, 0, 0);

        #region IO Utilities
        public static bool CreateFile(string path, DlogGraphObject dlogObject, bool refreshAsset = true) {
            if (dlogObject == null || string.IsNullOrEmpty(path)) return false;

            var assetGuid = AssetDatabase.AssetPathToGUID(path);
            dlogObject.GraphData.AssetGuid = assetGuid;

            CreateFileNoUpdate(path, dlogObject, refreshAsset);
            return true;
        }

        public static void CreateFileNoUpdate(string path, DlogGraphObject dlogObject, bool refreshAsset = true) {
            var jsonString = JsonUtility.ToJson(dlogObject.GraphData, true);
            File.WriteAllText(path, jsonString);
            if (refreshAsset) AssetDatabase.ImportAsset(path);
        }

        public static bool SaveGraph(DlogGraphObject dlogObject, bool refreshAsset = true) {
            if (dlogObject == null) return false;
            if (string.IsNullOrEmpty(dlogObject.GraphData.AssetGuid)) return false;

            var assetPath = AssetDatabase.GUIDToAssetPath(dlogObject.GraphData.AssetGuid);
            if (string.IsNullOrEmpty(assetPath)) return false;

            var jsonString = JsonUtility.ToJson(dlogObject.GraphData, true);
            File.WriteAllText(assetPath, jsonString);
            if (refreshAsset) AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return true;
        }

        public static DlogGraphObject LoadGraphAtPath(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) return null;

            var jsonString = File.ReadAllText(assetPath);
            try {
                var dlogData = JsonUtility.FromJson<DlogGraphData>(jsonString);
                var dlogObject = ScriptableObject.CreateInstance<DlogGraphObject>();
                dlogObject.Initialize(dlogData);
                dlogObject.AssetGuid = dlogData.AssetGuid;
                return dlogObject;
            } catch (ArgumentNullException exception) {
                Debug.LogException(exception);
                return null;
            }
        }

        public static DlogGraphObject LoadGraphAtGuid(string assetGuid) {
            if (string.IsNullOrEmpty(assetGuid)) return null;

            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            if (string.IsNullOrEmpty(assetPath)) return null;

            return LoadGraphAtPath(assetPath);
        }

        public static JObject LoadJObjectAtPath(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) return null;

            var jsonString = File.ReadAllText(assetPath);
            try {
                return JObject.Parse(jsonString);
            } catch (ArgumentNullException exception) {
                Debug.LogException(exception);
                return null;
            }
        }

        public static DlogGraphObject FromJObject(JObject converted) {
            if (converted == null) return null;

            var dlogData = JsonUtility.FromJson<DlogGraphData>(converted.ToString());
            var dlogObject = ScriptableObject.CreateInstance<DlogGraphObject>();
            dlogObject.Initialize(dlogData);
            dlogObject.AssetGuid = dlogData.AssetGuid;
            return dlogObject;
        }

        #endregion

        /// <summary>
        /// Converts (back-ports or forward-ports) dlogObject from <paramref name="fromVersion"/> to the current version.
        /// </summary>
        /// <param name="fromVersion">DialogueGraph object version</param>
        /// <param name="jsonObject">JObject representation of DialogueGraph to be converted</param>
        public static JObject VersionConvert(SemVer fromVersion, JObject jsonObject) {
            return VersionConverter.Convert(fromVersion, LatestVersion, jsonObject);
        }

        /// <summary>
        /// Returns <c>true</c> if the asset at <paramref name="assetPath"/> has a mismatching version.
        /// </summary>
        public static bool VersionMismatch(string assetPath) {
            JObject jsonObject = LoadJObjectAtPath(assetPath);
            if (jsonObject == null) return true;

            SemVer version = (SemVer)jsonObject.Value<string>("DialogueGraphVersion");
            return version != LatestVersion;
        }

        /**
         * Found this nifty method inside the codebase of ShaderGraph while reverse engineering some functionality.
         * I needed something like this so it didn't make sense to reinvent the wheel, so I took this and slightly modified it.
         * The original can be found in your unity project at: {PROJECT_ROOT}/Library/PackageCache/com.unity.shadergraph@{YOUR_SHADERGRAPH_VERSION}/Editor/Data/Util/GraphUtil.cs 
         */
        /// <summary>
        /// Sanitizes a supplied string such that it does not collide
        /// with any other name in a collection.
        /// </summary>
        /// <param name="existingNames">
        /// A collection of names that the new name should not collide with.
        /// </param>
        /// <param name="duplicateFormat">
        /// The format applied to the name if a duplicate exists.
        /// This must be a format string that contains `{0}` and `{1}`
        /// once each. An example could be `{0} ({1})`, which will append ` (n)`
        /// to the name for the n`th duplicate.
        /// </param>
        /// <param name="name">
        /// The name to be sanitized.
        /// </param>
        /// <returns>
        /// A name that is distinct form any name in `existingNames`.
        /// </returns>
        public static string SanitizeName(IEnumerable<string> existingNames, string duplicateFormat, string name) {
            var existingNamesList = existingNames.ToList();
            if (!existingNamesList.ToList().Contains(name))
                return name;

            var escapedDuplicateFormat = Regex.Escape(duplicateFormat);

            // Escaped format will escape string interpolation, so the escape caracters must be removed for these.
            escapedDuplicateFormat = escapedDuplicateFormat.Replace(@"\{0}", @"{0}");
            escapedDuplicateFormat = escapedDuplicateFormat.Replace(@"\{1}", @"{1}");

            var baseRegex = new Regex(string.Format(escapedDuplicateFormat, @"^(.*)", @"(\d+)"));

            var baseMatch = baseRegex.Match(name);
            if (baseMatch.Success)
                name = baseMatch.Groups[1].Value;

            var baseNameExpression = string.Format(@"^{0}", Regex.Escape(name));
            var regex = new Regex(string.Format(escapedDuplicateFormat, baseNameExpression, @"(\d+)") + "$");

            var existingDuplicateNumbers = existingNamesList.Select(existingName => regex.Match(existingName)).Where(m => m.Success).Select(m => int.Parse(m.Groups[1].Value)).Where(n => n > 0).Distinct().ToList();

            var duplicateNumber = 1;
            existingDuplicateNumbers.Sort();
            if (existingDuplicateNumbers.Any() && existingDuplicateNumbers.First() == 1) {
                duplicateNumber = existingDuplicateNumbers.Last() + 1;
                for (var i = 1; i < existingDuplicateNumbers.Count; i++) {
                    if (existingDuplicateNumbers[i - 1] != existingDuplicateNumbers[i] - 1) {
                        duplicateNumber = existingDuplicateNumbers[i - 1] + 1;
                        break;
                    }
                }
            }

            return string.Format(duplicateFormat, name, duplicateNumber);
        }
    }
}