using System;
using UnityEditor;
using UnityEngine;

namespace DialogueGraph {
    public class DlogGraphObject : ScriptableObject, ISerializationCallbackReceiver {
        [NonSerialized] private DlogGraphData m_GraphData;
        [NonSerialized] private int m_ObjectVersion;

        [SerializeField] public string AssetGuid;
        [SerializeField] public bool IsBlackboardVisible;
        [SerializeField] private string serializedGraph;
        [SerializeField] private int fileVersion;
        [SerializeField] private bool isDirty;

        public DlogGraphData GraphData {
            get => this.m_GraphData;
            private set {
                this.m_GraphData = value;
                if (this.m_GraphData != null) {
                    this.m_GraphData.Owner = this;
                }
            }
        }

        public void Initialize(DlogGraphData graphData) {
            this.GraphData = graphData;
            this.IsBlackboardVisible = this.m_GraphData.IsBlackboardVisible;
        }

        public bool IsDirty {
            get => isDirty;
            set => isDirty = value;
        }

        public bool WasUndoRedoPerformed => this.m_ObjectVersion != this.fileVersion;

        public void RegisterCompleteObjectUndo(string operationName) {
            Undo.RegisterCompleteObjectUndo(this, operationName);
            this.fileVersion++;
            this.m_ObjectVersion++;
            this.isDirty = true;
        }

        public void OnBeforeSerialize() {
            if(this.m_GraphData == null) return;

            this.serializedGraph = JsonUtility.ToJson(this.m_GraphData, true);
            this.AssetGuid = this.m_GraphData.AssetGuid;
        }

        public void OnAfterDeserialize() {
            if(this.m_GraphData != null) return;
            this.GraphData = Deserialize();
        }

        public void HandleUndoRedo() {
            if (!this.WasUndoRedoPerformed) {
                Debug.LogError("Trying to handle undo/redo when undo/redo was not performed", this);
                return;
            }

            DlogGraphData graphData = Deserialize();
            this.m_GraphData.ReplaceWith(graphData);
        }

        private DlogGraphData Deserialize() {
            DlogGraphData graphData = JsonUtility.FromJson<DlogGraphData>(this.serializedGraph);
            graphData.AssetGuid = this.AssetGuid;
            this.m_ObjectVersion = this.fileVersion;
            this.serializedGraph = "";
            return graphData;
        }

        public void RecalculateAssetGuid(string assetPath) {
            this.AssetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            this.m_GraphData.AssetGuid = this.AssetGuid;
        }
    }
}