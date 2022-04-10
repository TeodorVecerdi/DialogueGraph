using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DialogueGraph {
    [CustomEditor(typeof(DlogGraphImporter))]
    public class DlogGraphImporterEditor : ScriptedImporterEditor {
        protected override bool needsApplyRevert => false;

        private static GUIStyle titleStyle;
        private static GUIStyle wrapLabelStyle;

        public override void OnInspectorGUI() {
            var importer = target as DlogGraphImporter;

            if (assetTarget is VersionMismatchObject) {
                if (titleStyle == null) {
                    titleStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 20,
                        fontStyle = FontStyle.Bold,
                    };
                }

                if (wrapLabelStyle == null) {
                    wrapLabelStyle = new GUIStyle(EditorStyles.label) {
                        wordWrap = true,
                    };
                }
                GUILayout.Label("Version Mismatch", titleStyle);
                GUILayout.Label("Unable to load graph due to version mismatch. Before using you must convert the graph to the latest version.", wrapLabelStyle);
                GUILayout.Space(8.0f);

                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;

                if (GUILayout.Button("Convert Graph", GUILayout.Height(28.0f))) {
                    JObject jsonObject = DialogueGraphUtility.LoadJObjectAtPath(importer.assetPath);
                    if (jsonObject != null) {
                        SemVer fileVersion = (SemVer)jsonObject.Value<string>("DialogueGraphVersion");
                        JObject converted = DialogueGraphUtility.VersionConvert(fileVersion, jsonObject);
                        DlogGraphObject dlogObject = DialogueGraphUtility.FromJObject(converted);
                        DialogueGraphUtility.SaveGraph(dlogObject);
                    }
                }

                GUI.backgroundColor = oldColor;

                return;
            }

            if (GUILayout.Button("Open Dialogue Graph Editor")) {
                OpenEditorWindow(importer.assetPath);
            }

            ApplyRevertGUI();
        }

        public static bool OpenEditorWindow(string assetPath) {
            var extension = Path.GetExtension(assetPath);
            if (string.IsNullOrEmpty(extension))
                return false;

            extension = extension.Substring(1).ToLowerInvariant();
            if (extension != DlogGraphImporter.Extension)
                return false;

            JObject jsonObject = DialogueGraphUtility.LoadJObjectAtPath(assetPath);
            if (jsonObject == null)
                return false;

            SemVer fileVersion = (SemVer)jsonObject.Value<string>("DialogueGraphVersion");
            var comparison = fileVersion.CompareTo(DialogueGraphUtility.LatestVersion);
            if (comparison >= 0) {
                DlogGraphObject dlogObject = DialogueGraphUtility.FromJObject(jsonObject);
                OpenEditorWindow(dlogObject, assetPath);
                return true;
            }

            if (EditorUtility.DisplayDialog("Version mismatch", "The graph you are trying to load was saved with an older version of Dialogue Graph.\nIf you " +
                                                                "proceed with loading it will be converted to the current version. (A backup will be created)\n\nDo you wish " +
                                                                "to continue?", "Yes", "No")) {
                JObject converted = DialogueGraphUtility.VersionConvert(fileVersion, jsonObject);
                DlogGraphObject dlogObject = DialogueGraphUtility.FromJObject(converted);
                DialogueGraphUtility.SaveGraph(dlogObject);
                OpenEditorWindow(dlogObject, assetPath);
                return true;
            }

            return false;
        }

        private static void OpenEditorWindow(DlogGraphObject dlogObject, string assetPath) {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(dlogObject.AssetGuid)) {
                dlogObject.RecalculateAssetGuid(assetPath);
                DialogueGraphUtility.SaveGraph(dlogObject, false);
            }

            foreach (var activeWindow in Resources.FindObjectsOfTypeAll<DlogEditorWindow>()) {
                if (activeWindow.SelectedAssetGuid != guid)
                    continue;

                // TODO: Ask user if they want to replace the current window (maybe ask to save before opening with cancel button)
                activeWindow.SetDlogObject(dlogObject);
                activeWindow.BuildWindow();
                activeWindow.Focus();
                return;
            }

            var window = EditorWindow.CreateWindow<DlogEditorWindow>(typeof(DlogEditorWindow), typeof(SceneView));
            window.titleContent = EditorGUIUtility.TrTextContentWithIcon(guid, Resources.Load<Texture2D>(ResourcesUtility.IconSmall));
            window.SetDlogObject(dlogObject);
            window.BuildWindow();
            window.Focus();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line) {
            var path = AssetDatabase.GetAssetPath(instanceID);
            var extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
                return false;

            extension = extension.Substring(1).ToLowerInvariant();
            if (extension != DlogGraphImporter.Extension)
                return false;

            OpenEditorWindow(path);
            return true;
        }
    }
}