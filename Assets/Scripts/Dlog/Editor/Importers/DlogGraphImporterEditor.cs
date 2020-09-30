using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Dlog {
    [CustomEditor(typeof(DlogGraphImporter))]
    public class DlogGraphImporterEditor : ScriptedImporterEditor{
        protected override bool needsApplyRevert => false;

        
        public override void OnInspectorGUI() {
            if (GUILayout.Button("Open Dlog Editor"))
            {
                var importer = target as AssetImporter;
                Debug.Assert(importer != null, "importer != null");
                OpenEditorWindow(importer.assetPath);
            }            
            ApplyRevertGUI();
        }

        public static bool OpenEditorWindow(string assetPath) {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var extension = Path.GetExtension(assetPath);
            if (string.IsNullOrEmpty(extension))
                return false;
            
            extension = extension.Substring(1).ToLowerInvariant();
            if (extension != DlogGraphImporter.Extension)
                return false;

            var dlogObject = SaveUtility.LoadAtPath(assetPath);
            if (string.IsNullOrEmpty(dlogObject.AssetGuid)) {
                dlogObject.RecalculateAssetGuid(assetPath);
                SaveUtility.Save(dlogObject);
            }

            foreach (var activeWindow in Resources.FindObjectsOfTypeAll<DlogEditorWindow>()) {
                if (activeWindow.SelectedAssetGuid != guid)
                    continue;

                activeWindow.SetDlogObject(dlogObject);
                activeWindow.Refresh();
                activeWindow.Focus();
                return true;
            }

            var window = EditorWindow.CreateWindow<DlogEditorWindow>(typeof(DlogEditorWindow), typeof(SceneView));
            window.titleContent = EditorGUIUtility.TrTextContentWithIcon(guid, Resources.Load<Texture2D>(ResourcesUtility.IconSmall));
            window.SetDlogObject(dlogObject);
            window.BuildWindow();
            window.Focus();
            return true;
        }
        
        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var path = AssetDatabase.GetAssetPath(instanceID);
            return OpenEditorWindow(path);
        }
    }
}