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
                if (m_Deleted) return false;
                if (GraphObject == null) return false;
                string current = JsonUtility.ToJson(GraphObject.GraphData, true);
                string saved = File.ReadAllText(AssetDatabase.GUIDToAssetPath(SelectedAssetGuid));
                return !string.Equals(current, saved, StringComparison.Ordinal);
            }
        }

        public void BuildWindow() {
            rootVisualElement.Clear();
            Events = new DlogWindowEvents { SaveRequested = SaveAsset, SaveAsRequested = SaveAs, ShowInProjectRequested = ShowInProject };

            m_EditorView = new EditorView(this) {
                name = "Dialogue Graph",
                IsBlackboardVisible = GraphObject.IsBlackboardVisible,
            };
            rootVisualElement.Add(m_EditorView);
            Refresh();
        }

        private void Update() {
            if (focusedWindow == this && m_Deleted) {
                DisplayDeletedFromDiskDialog();
            }

            if (GraphObject == null && SelectedAssetGuid != null) {
                string assetGuid = SelectedAssetGuid;
                SelectedAssetGuid = null;
                DlogGraphObject newObject = DialogueGraphUtility.LoadGraphAtGuid(assetGuid);
                SetGraphObject(newObject);
                Refresh();
            }

            if (GraphObject == null) {
                Close();
                return;
            }

            if (m_EditorView == null && GraphObject != null) {
                BuildWindow();
            }

            if (m_EditorView == null) {
                Close();
                return;
            }

            bool wasUndoRedoPerformed = GraphObject.WasUndoRedoPerformed;
            if (wasUndoRedoPerformed) {
                m_EditorView.HandleChanges();
                GraphObject.GraphData.ClearChanges();
                GraphObject.HandleUndoRedo();
            }

            if (GraphObject.IsDirty || wasUndoRedoPerformed) {
                UpdateTitle();
                GraphObject.IsDirty = false;
            }

            m_EditorView.HandleChanges();
            GraphObject.GraphData.ClearChanges();
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
                m_Deleted = false;
            }
        }

        public void SetGraphObject(DlogGraphObject graphObject) {
            SelectedAssetGuid = graphObject.AssetGuid;
            GraphObject = graphObject;
        }

        public void Refresh() {
            UpdateTitle();

            m_EditorView ??= rootVisualElement.Q<EditorView>();

            if (m_EditorView == null) {
                BuildWindow();
            }

            m_EditorView!.BuildGraph();
        }

        public void GraphDeleted() {
            m_Deleted = true;
        }

        private void UpdateTitle() {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(SelectedAssetGuid));
            titleContent.text = asset.name.Split('/').Last() + (IsDirty ? "*" : "");
        }

        void OnGUI()
        {
            if (Event.current != null)
            {
                if (Event.current.type == EventType.ExecuteCommand)
                {
                    if (Event.current.commandName == "Save")
                    {
                    }
                }
            }
        }

        private void OnEnable()
        {
            this.SetAntiAliasing(4);
        }

        private void OnDestroy() {
            if (!m_SkipOnDestroyCheck && IsDirty && EditorUtility.DisplayDialog("Dialogue Graph has been modified", "Do you want to save the changes you made in the Dialogue Graph?\nYour changes will be lost if you don't save them.", "Save", "Don't Save")) {
                SaveAsset();
            }
        }

        public override void SaveChanges()
        {
            SaveAsset();
        }

        #region Window Events
        private void SaveAsset() {
            GraphObject.GraphData.DialogueGraphVersion = DialogueGraphUtility.LatestVersion;
            DialogueGraphUtility.SaveGraph(GraphObject);
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