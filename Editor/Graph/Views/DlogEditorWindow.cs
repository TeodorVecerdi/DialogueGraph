using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DialogueGraph {
    public class DlogEditorWindow : EditorWindow {
        private string selectedAssetGuid;
        private DlogGraphObject dlogObject;
        private DlogWindowEvents windowEvents;
        private EditorView editorView;

        private bool deleted;
        private bool skipOnDestroyCheck;

        public string SelectedAssetGuid {
            get => selectedAssetGuid;
            set => selectedAssetGuid = value;
        }
        public DlogGraphObject GraphObject => dlogObject;
        public DlogWindowEvents Events => windowEvents;

        public bool IsDirty {
            get {
                if (deleted) return false;
                if (dlogObject == null) return false;
                var current = JsonUtility.ToJson(dlogObject.DlogGraph, true);
                var saved = File.ReadAllText(AssetDatabase.GUIDToAssetPath(selectedAssetGuid));
                return !string.Equals(current, saved, StringComparison.Ordinal);
            }
        }

        public void BuildWindow() {
            rootVisualElement.Clear();
            windowEvents = new DlogWindowEvents {SaveRequested = SaveAsset, SaveAsRequested = SaveAs, ShowInProjectRequested = ShowInProject};

            editorView = new EditorView(this, dlogObject) {
                name = "Dlog Graph",
                IsBlackboardVisible = dlogObject.IsBlackboardVisible
            };
            rootVisualElement.Add(editorView);
            Refresh();
        }

        private void Update() {
            if (focusedWindow == this && deleted) {
                DisplayDeletedFromDiskDialog();
            }

            if (dlogObject == null && selectedAssetGuid != null) {
                var assetGuid = selectedAssetGuid;
                selectedAssetGuid = null;
                var newObject = DialogueGraphUtility.LoadGraphAtGuid(assetGuid);
                SetDlogObject(newObject);
                Refresh();
            }

            if (dlogObject == null) {
                Close();
                return;
            }

            if (editorView == null && dlogObject != null) {
                BuildWindow();
            }

            if (editorView == null) {
                Close();
            }

            var wasUndoRedoPerformed = dlogObject.WasUndoRedoPerformed;
            if (wasUndoRedoPerformed) {
                editorView.HandleChanges();
                dlogObject.DlogGraph.ClearChanges();
                dlogObject.HandleUndoRedo();
            }

            if (dlogObject.IsDirty || wasUndoRedoPerformed) {
                UpdateTitle();
                dlogObject.IsDirty = false;
            }

            editorView.HandleChanges();
            dlogObject.DlogGraph.ClearChanges();
        }

        private void DisplayDeletedFromDiskDialog() {
            bool shouldClose = true; // Close unless if the same file was replaced

            if (EditorUtility.DisplayDialog("Dialogue Graph Missing", $"{AssetDatabase.GUIDToAssetPath(selectedAssetGuid)} has been deleted or moved outside of Unity.\n\nWould you like to save your Graph Asset?", "Save As", "Close Window")) {
                shouldClose = !SaveAs();
            }

            if (shouldClose)
                Close();
            else
                deleted = false; // Was restored
        }

        public void SetDlogObject(DlogGraphObject dlogObject) {
            SelectedAssetGuid = dlogObject.AssetGuid;
            this.dlogObject = dlogObject;
        }

        public void Refresh() {
            UpdateTitle();
            editorView.BuildGraph();
        }

        public void GraphDeleted() {
            deleted = true;
        }

        private void UpdateTitle() {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(selectedAssetGuid));
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
            dlogObject.DlogGraph.DialogueGraphVersion = DialogueGraphUtility.LatestVersion;
            DialogueGraphUtility.SaveGraph(dlogObject);
            UpdateTitle();
        }

        private bool SaveAs() {
            if (!string.IsNullOrEmpty(selectedAssetGuid) && dlogObject != null) {
                var assetPath = AssetDatabase.GUIDToAssetPath(selectedAssetGuid);
                if (string.IsNullOrEmpty(assetPath) || dlogObject == null)
                    return false;

                var directoryPath = Path.GetDirectoryName(assetPath);
                var savePath = EditorUtility.SaveFilePanelInProject("Save As...", Path.GetFileNameWithoutExtension(assetPath), DlogGraphImporter.Extension, "", directoryPath);
                savePath = savePath.Replace(Application.dataPath, "Assets");
                if (savePath != directoryPath) {
                    if (!string.IsNullOrEmpty(savePath)) {
                        if (DialogueGraphUtility.CreateFile(savePath, dlogObject)) {
                            dlogObject.RecalculateAssetGuid(savePath);
                            DlogGraphImporterEditor.OpenEditorWindow(savePath);
                        }
                    }

                    dlogObject.IsDirty = false;
                    return false;
                }

                SaveAsset();
                dlogObject.IsDirty = false;
                return true;
            }

            return false;
        }

        private void ShowInProject() {
            if (string.IsNullOrEmpty(selectedAssetGuid)) return;

            var path = AssetDatabase.GUIDToAssetPath(selectedAssetGuid);
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUIUtility.PingObject(asset);
        }
        #endregion
    }
}