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
        public DlogGraphView Graph;

        public bool IsDirty {
            get {
                var current = JsonUtility.ToJson(dlogObject.DlogGraph, true);
                var saved = File.ReadAllText(AssetDatabase.GUIDToAssetPath(selectedAssetGuid));
                return !string.Equals(current, saved, StringComparison.Ordinal);
            }
        }
        private string selectedAssetGuid;
        private DlogGraphObject dlogObject;
        private DlogWindowEvents windowEvents;

        public string SelectedAssetGuid {
            get => selectedAssetGuid;
            set => selectedAssetGuid = value;
        }

        public void BuildWindow() {
            windowEvents = new DlogWindowEvents();
            windowEvents.SaveRequested += SaveAsset;
            windowEvents.SaveAsRequested += SaveAs;
            windowEvents.ShowInProjectRequested += ShowInProject;
            
            BuildGraph();
            var toolbar = BuildToolbar();
            rootVisualElement.Add(toolbar);
            
            Refresh();
        }

        public void SetDlogObject(DlogGraphObject dlogObject) {
            SelectedAssetGuid = dlogObject.AssetGuid;
            this.dlogObject = dlogObject;
        }

        public void Refresh() {
            UpdateTitle();
        }

        private void UpdateTitle() {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(selectedAssetGuid));
            titleContent.text = asset.name.Split('/').Last() + (IsDirty ? "*" : "");
        }

        private void BuildGraph() {
            Graph = new DlogGraphView(this) {
                name = "Dlog Graph",
                IsBlackboardVisible = dlogObject.IsBlackboardVisible
            };
            Graph.StretchToParentSize();

            var graphStyle = Graph.style;
            graphStyle.top = 21f;
            rootVisualElement.Add(Graph);
        }

        private IMGUIContainer BuildToolbar() {
            var toolbar = new IMGUIContainer(() => {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button("Save Graph", EditorStyles.toolbarButton)) {
                    windowEvents.SaveRequested?.Invoke();
                }
                GUILayout.Space(6);
                if (GUILayout.Button("Save As...", EditorStyles.toolbarButton)) {
                    windowEvents.SaveAsRequested?.Invoke();
                }
                GUILayout.Space(6);
                if (GUILayout.Button("Show In Project", EditorStyles.toolbarButton)) {
                    windowEvents.ShowInProjectRequested?.Invoke();
                }
                
                GUILayout.FlexibleSpace();
                Graph.IsBlackboardVisible = GUILayout.Toggle(Graph.IsBlackboardVisible, "Blackboard", EditorStyles.toolbarButton);
                dlogObject.IsBlackboardVisible = Graph.IsBlackboardVisible;

                GUILayout.EndHorizontal();
            });
            return toolbar;
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