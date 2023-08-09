using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DialogueGraph {
    public class DlogEditorWindow : EditorWindow {
        private EditorView editorView;

        private bool deleted;
        private bool skipOnDestroyCheck;

        public string SelectedAssetGuid { get; set; }

        public DlogGraphObject GraphObject { get; private set; }

        public DlogWindowEvents Events { get; private set; }

        public bool IsDirty {
            get {
                if (deleted) return false;
                if (GraphObject == null) return false;
                var current = JsonUtility.ToJson(GraphObject.DlogGraph, true);
                var saved = File.ReadAllText(AssetDatabase.GUIDToAssetPath(SelectedAssetGuid));
                return !string.Equals(current, saved, StringComparison.Ordinal);
            }
        }

        public void BuildWindow() {
            rootVisualElement.Clear();
            Events = new DlogWindowEvents {SaveRequested = SaveAsset, SaveAsRequested = SaveAs, ShowInProjectRequested = ShowInProject};

            editorView = new EditorView(this) {
                name = "Dlog Graph",
                IsBlackboardVisible = GraphObject.IsBlackboardVisible
            };
            rootVisualElement.Add(editorView);
            Refresh();
        }

        private void Update() {
            if (focusedWindow == this && deleted) {
                DisplayDeletedFromDiskDialog();
            }

            if (GraphObject == null && SelectedAssetGuid != null) {
                var assetGuid = SelectedAssetGuid;
                SelectedAssetGuid = null;
                var newObject = DialogueGraphUtility.LoadGraphAtGuid(assetGuid);
                SetDlogObject(newObject);
                Refresh();
            }

            if (GraphObject == null) {
                Close();
                return;
            }

            if (editorView == null && GraphObject != null) {
                BuildWindow();
            }

            if (editorView == null) {
                Close();
            }

            var wasUndoRedoPerformed = GraphObject.WasUndoRedoPerformed;
            if (wasUndoRedoPerformed) {
                editorView.HandleChanges();
                GraphObject.DlogGraph.ClearChanges();
                GraphObject.HandleUndoRedo();
            }

            if (GraphObject.IsDirty || wasUndoRedoPerformed) {
                UpdateTitle();
                GraphObject.IsDirty = false;
            }

            editorView.HandleChanges();
            GraphObject.DlogGraph.ClearChanges();
        }

        private void DisplayDeletedFromDiskDialog() {
            bool shouldClose = true; // Close unless if the same file was replaced

            if (EditorUtility.DisplayDialog("Dialogue Graph Missing", $"{AssetDatabase.GUIDToAssetPath(SelectedAssetGuid)} has been deleted or moved outside of Unity.\n\nWould you like to save your Graph Asset?", "Save As", "Close Window")) {
                shouldClose = !SaveAs();
            }

            if (shouldClose)
                Close();
            else
                deleted = false; // Was restored
        }

        public void SetDlogObject(DlogGraphObject dlogObject) {
            SelectedAssetGuid = dlogObject.AssetGuid;
            GraphObject = dlogObject;
        }

        public void Refresh() {
            UpdateTitle();

            if (editorView == null) {
                editorView = rootVisualElement.Q<EditorView>();
            }

            if (editorView == null) {
                BuildWindow();
            }

            editorView.BuildGraph();
        }

        public void GraphDeleted() {
            deleted = true;
        }

        private void UpdateTitle() {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(SelectedAssetGuid));
            titleContent.text = asset.name.Split('/').Last() + (IsDirty ? "*" : "");
        }

        private void OnEnable() {
            this.SetAntiAliasing(4);
        }

        private void OnDestroy() {
            if (!skipOnDestroyCheck && IsDirty && EditorUtility.DisplayDialog("Dialogue Graph has been modified", "Do you want to save the changes you made in the Dialogue Graph?\nYour changes will be lost if you don't save them.", "Save", "Don't Save")) {
                SaveAsset();
            }
        }

        #region Window Events
        private void SaveAsset() {
            GraphObject.DlogGraph.DialogueGraphVersion = DialogueGraphUtility.LatestVersion;
            DialogueGraphUtility.SaveGraph(GraphObject);
            UpdateTitle();
        }

        private bool SaveAs() {
            if (!string.IsNullOrEmpty(SelectedAssetGuid) && GraphObject != null) {
                var assetPath = AssetDatabase.GUIDToAssetPath(SelectedAssetGuid);
                if (string.IsNullOrEmpty(assetPath) || GraphObject == null)
                    return false;

                var directoryPath = Path.GetDirectoryName(assetPath);
                var savePath = EditorUtility.SaveFilePanelInProject("Save As...", Path.GetFileNameWithoutExtension(assetPath), DlogGraphImporter.Extension, "", directoryPath);
                savePath = savePath.Replace(Application.dataPath, "Assets");
                if (savePath != directoryPath) {
                    if (!string.IsNullOrEmpty(savePath)) {
                        if (DialogueGraphUtility.CreateFile(savePath, GraphObject)) {
                            GraphObject.RecalculateAssetGuid(savePath);
                            DlogGraphImporterEditor.OpenEditorWindow(savePath);
                        }
                    }

                    GraphObject.IsDirty = false;
                    return false;
                }

                SaveAsset();
                GraphObject.IsDirty = false;
                return true;
            }

            return false;
        }

        private void ShowInProject() {
            if (string.IsNullOrEmpty(SelectedAssetGuid)) return;

            var path = AssetDatabase.GUIDToAssetPath(SelectedAssetGuid);
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUIUtility.PingObject(asset);
        }
        #endregion
    }
}