using System;
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

        private static GUIStyle s_TitleStyle;
        private static GUIStyle s_WrapLabelStyle;

        public override void OnInspectorGUI() {
            DlogGraphImporter importer = target as DlogGraphImporter;

            if (assetTarget is VersionMismatchObject) {
                s_TitleStyle ??= new GUIStyle(EditorStyles.label) { fontSize = 20, fontStyle = FontStyle.Bold };
                s_WrapLabelStyle ??= new GUIStyle(EditorStyles.label) { wordWrap = true };

                GUILayout.Label("Version Mismatch", s_TitleStyle);
                GUILayout.Label("Unable to load graph due to version mismatch. Before using you must convert the graph to the latest version.", s_WrapLabelStyle);
                GUILayout.Space(8.0f);

                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;

                if (GUILayout.Button("Convert Graph", GUILayout.Height(28.0f))) {
                    JObject jsonObject = DialogueGraphUtility.LoadJObjectAtPath(importer!.assetPath);
                    if (jsonObject != null) {
                        SemVer fileVersion = (SemVer)jsonObject.Value<string>("DialogueGraphVersion");
                        JObject converted = DialogueGraphUtility.VersionConvert(fileVersion, jsonObject);
                        DlogGraphObject graphObject = DialogueGraphUtility.FromJObject(converted);
                        DialogueGraphUtility.SaveGraph(graphObject);
                    }
                }

                GUI.backgroundColor = oldColor;

                return;
            }

            if (GUILayout.Button("Open Dialogue Graph Editor")) {
                OpenEditorWindow(importer!.assetPath);
            }

            ApplyRevertGUI();
        }

        public static bool OpenEditorWindow(string assetPath) {
            string extension = Path.GetExtension(assetPath);
            if (string.IsNullOrEmpty(extension)) return false;

            extension = extension[1..].ToLowerInvariant();
            if (extension != DlogGraphImporter.EXTENSION) return false;

            JObject jsonObject = DialogueGraphUtility.LoadJObjectAtPath(assetPath);
            if (jsonObject == null) return false;

            SemVer fileVersion = (SemVer) jsonObject.Value<string>("DialogueGraphVersion");
            if (fileVersion >= DialogueGraphUtility.LatestVersion) {
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

        private static void OpenEditorWindow(DlogGraphObject graphObject, string assetPath) {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(graphObject.AssetGuid)) {
                graphObject.RecalculateAssetGuid(assetPath);
                DialogueGraphUtility.SaveGraph(graphObject, false);
            }

            foreach (DlogEditorWindow activeWindow in Resources.FindObjectsOfTypeAll<DlogEditorWindow>()) {
                if (activeWindow.SelectedAssetGuid != guid)
                    continue;

                // TODO: Ask user if they want to replace the current window (maybe ask to save before opening with cancel button)
                activeWindow.SetGraphObject(graphObject);
                activeWindow.BuildWindow();
                activeWindow.Focus();
                return;
            }

            DlogEditorWindow window = EditorWindow.CreateWindow<DlogEditorWindow>(typeof(DlogEditorWindow), typeof(SceneView));
            window.titleContent = EditorGUIUtility.TrTextContentWithIcon(guid, Resources.Load<Texture2D>(ResourcesUtility.ICON_SMALL));
            window.SetGraphObject(graphObject);
            window.BuildWindow();
            window.Focus();
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceID, int line) {
            string path = AssetDatabase.GetAssetPath(instanceID);

            string extension = Path.GetExtension(path)?[1..];
            if (!string.Equals(extension, DlogGraphImporter.EXTENSION, StringComparison.OrdinalIgnoreCase))
                return false;

            OpenEditorWindow(path);
            return true;
        }
    }
}