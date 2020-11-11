using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Dlog {
    public class DlogGraphAssetPostprocessor : AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            var anyRemovedAssets = deletedAssets.Any(path => path.EndsWith(DlogGraphImporter.Extension, StringComparison.InvariantCultureIgnoreCase));
            if (anyRemovedAssets)
                DisplayDeletionDialog(deletedAssets);
        }

        private static void DisplayDeletionDialog(string[] deletedAssets) {
            var windows = Resources.FindObjectsOfTypeAll<DlogEditorWindow>();
            foreach (var window in windows) {
                foreach (var asset in deletedAssets) {
                    if (window.SelectedAssetGuid == AssetDatabase.AssetPathToGUID(asset)) {
                        window.GraphDeleted();
                    }
                }
            }
        }
    }
}