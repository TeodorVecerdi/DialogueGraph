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
        private EditorView m_EditorView;
        private bool m_Deleted;
        private bool m_SkipOnDestroyCheck;

        public string SelectedAssetGuid { get; set; }
        public DlogGraphObject GraphObject { get; private set; }
        public DlogWindowEvents Events { get; private set; }

        public bool IsDirty {
            get {
                if (this.m_Deleted) return false;
                if (this.GraphObject == null) return false;
                string current = JsonUtility.ToJson(GraphObject.GraphData, true);
                string saved = File.ReadAllText(AssetDatabase.GUIDToAssetPath(SelectedAssetGuid));
                return !string.Equals(current, saved, StringComparison.Ordinal);
            }
        }

        public void BuildWindow() {
            this.rootVisualElement.Clear();
            this.Events = new DlogWindowEvents { SaveRequested = SaveAsset, SaveAsRequested = SaveAs, ShowInProjectRequested = ShowInProject };

            this.m_EditorView = new EditorView(this) {
                name = "Dialogue Graph",
                IsBlackboardVisible = this.GraphObject.IsBlackboardVisible,
            };
            this.rootVisualElement.Add(this.m_EditorView);
            Refresh();
        }

        private void Update() {
            if (focusedWindow == this && this.m_Deleted) {
                DisplayDeletedFromDiskDialog();
            }

            if (this.GraphObject == null && this.SelectedAssetGuid != null) {
                string assetGuid = this.SelectedAssetGuid;
                this.SelectedAssetGuid = null;
                DlogGraphObject newObject = DialogueGraphUtility.LoadGraphAtGuid(assetGuid);
                this.SetGraphObject(newObject);
                Refresh();
            }

            if (this.GraphObject == null) {
                Close();
                return;
            }

            if (this.m_EditorView == null && this.GraphObject != null) {
                BuildWindow();
            }

            if (this.m_EditorView == null) {
                Close();
                return;
            }

            bool wasUndoRedoPerformed = this.GraphObject.WasUndoRedoPerformed;
            if (wasUndoRedoPerformed) {
                this.m_EditorView.HandleChanges();
                this.GraphObject.GraphData.ClearChanges();
                this.GraphObject.HandleUndoRedo();
            }

            if (this.GraphObject.IsDirty || wasUndoRedoPerformed) {
                UpdateTitle();
                this.GraphObject.IsDirty = false;
            }

            this.m_EditorView.HandleChanges();
            this.GraphObject.GraphData.ClearChanges();
        }

        private void DisplayDeletedFromDiskDialog() {
            // Close unless if the same file was replaced
            bool shouldClose = true;

            if (EditorUtility.DisplayDialog("Dialogue Graph Missing", $"{AssetDatabase.GUIDToAssetPath(SelectedAssetGuid)} has been deleted or moved outside of Unity.\n\nWould you like to save your Graph Asset?", "Save As", "Close Window")) {
                shouldClose = !SaveAs();
            }

            if (shouldClose) {
                Close();
            } else {
                // Was restored
                this.m_Deleted = false;
            }
        }

        public void SetGraphObject(DlogGraphObject graphObject) {
            this.SelectedAssetGuid = graphObject.AssetGuid;
            this.GraphObject = graphObject;
        }

        public void Refresh() {
            UpdateTitle();

            this.m_EditorView ??= this.rootVisualElement.Q<EditorView>();

            if (this.m_EditorView == null) {
                BuildWindow();
            }

            this.m_EditorView!.BuildGraph();
        }

        public void GraphDeleted() {
            this.m_Deleted = true;
        }

        private void UpdateTitle() {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(this.SelectedAssetGuid));
            titleContent.text = asset.name.Split('/').Last() + (this.IsDirty ? "*" : "");
        }

        private void OnEnable() {
            this.SetAntiAliasing(4);
        }

        private void OnDestroy() {
            if (!this.m_SkipOnDestroyCheck && this.IsDirty && EditorUtility.DisplayDialog("Dialogue Graph has been modified", "Do you want to save the changes you made in the Dialogue Graph?\nYour changes will be lost if you don't save them.", "Save", "Don't Save")) {
                SaveAsset();
            }
        }

        #region Window Events
        private void SaveAsset() {
            this.GraphObject.GraphData.DialogueGraphVersion = DialogueGraphUtility.LatestVersion;
            DialogueGraphUtility.SaveGraph(this.GraphObject);
            UpdateTitle();
        }

        private bool SaveAs() {
            if (!string.IsNullOrEmpty(SelectedAssetGuid) && GraphObject != null) {
                var assetPath = AssetDatabase.GUIDToAssetPath(SelectedAssetGuid);
                if (string.IsNullOrEmpty(assetPath) || GraphObject == null)
                    return false;

                var directoryPath = Path.GetDirectoryName(assetPath);
                var savePath = EditorUtility.SaveFilePanelInProject("Save As...", Path.GetFileNameWithoutExtension(assetPath), DlogGraphImporter.EXTENSION, "", directoryPath);
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