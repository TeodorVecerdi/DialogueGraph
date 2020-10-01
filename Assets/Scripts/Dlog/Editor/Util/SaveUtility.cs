using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Dlog {
    public static class SaveUtility {
        public static bool CreateFile(string path, DlogGraphObject dlogObject, bool refreshAsset = true) {
            if (dlogObject == null || string.IsNullOrEmpty(path)) return false;

            var assetGuid = AssetDatabase.AssetPathToGUID(path);
            dlogObject.DlogGraph.AssetGuid = assetGuid;

            var jsonString = JsonUtility.ToJson(dlogObject.DlogGraph, true);
            File.WriteAllText(path, jsonString);
            if (refreshAsset) AssetDatabase.ImportAsset(path);

            return true;
        }
        
        public static bool Save(DlogGraphObject dlogObject, bool refreshAsset = true) {
            if (dlogObject == null) return false;
            if (string.IsNullOrEmpty(dlogObject.DlogGraph.AssetGuid)) return false;

            var assetPath = AssetDatabase.GUIDToAssetPath(dlogObject.DlogGraph.AssetGuid);
            if (string.IsNullOrEmpty(assetPath)) return false;

            var jsonString = JsonUtility.ToJson(dlogObject.DlogGraph, true);
            File.WriteAllText(assetPath, jsonString);
            if (refreshAsset) AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            return true;
        }

        public static DlogGraphObject LoadAtPath(string assetPath) {
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

        public static DlogGraphObject LoadAtGuid(string assetGuid) {
            if (string.IsNullOrEmpty(assetGuid)) return null;

            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            if (string.IsNullOrEmpty(assetPath)) return null;

            return LoadAtPath(assetPath);
        }
    }
}