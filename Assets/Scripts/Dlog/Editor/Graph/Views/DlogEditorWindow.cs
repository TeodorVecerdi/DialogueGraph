using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Dlog {
    public class DlogEditorWindow : EditorWindow {

        private string selectedAssetGuid;
        private DlogGraphObject dlogObject;
        private DlogWindowEvents windowEvents;
        private EditorView editorView;

        private bool deleted;

        public string SelectedAssetGuid {
            get => selectedAssetGuid;
            set => selectedAssetGuid = value;
        }
        public DlogGraphObject GraphObject => dlogObject;
        public DlogWindowEvents Events => windowEvents;
        
        public bool IsDirty {
            get {
                if (deleted) return false;
                
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
                Debug.Log("Graph deleted");
                // TODO: Ask user if they want to save
                Close();
            }

            if (dlogObject == null) {
                Debug.Log("Graph Object is null");
                // TODO: Attempt to recover
                Close();
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
            if (IsDirty && EditorUtility.DisplayDialog("PLACEHOLDER TITLE [SAVE DIALOG]", "PLACEHOLDER MESSAGE [SAVE DIALOG]", "Save", "Don't Save")) {
                SaveAsset();
            }
        }

        #region Window Events
        private void SaveAsset() {
            SaveUtility.Save(dlogObject);
            UpdateTitle();
        }

        private void SaveAs() {
            if (!string.IsNullOrEmpty(selectedAssetGuid) && dlogObject != null) {
                var assetPath = AssetDatabase.GUIDToAssetPath(selectedAssetGuid);
                if(string.IsNullOrEmpty(assetPath) || dlogObject == null) 
                    return;

                var directoryPath = Path.GetDirectoryName(assetPath);
                var savePath = EditorUtility.SaveFilePanelInProject("Save As...", Path.GetFileNameWithoutExtension(assetPath), DlogGraphImporter.Extension, "", directoryPath);
                savePath = savePath.Replace(Application.dataPath, "Assets");
                if (savePath != directoryPath && !string.IsNullOrEmpty(savePath)) {
                    if (SaveUtility.CreateFile(savePath, dlogObject)) {
                        dlogObject.RecalculateAssetGuid(savePath);
                        DlogGraphImporterEditor.OpenEditorWindow(savePath);
                    }
                }
                dlogObject.IsDirty = false;
            } else {
                SaveAsset();
                dlogObject.IsDirty = false;
            }
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