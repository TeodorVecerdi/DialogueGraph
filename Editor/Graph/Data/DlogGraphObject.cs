using System;
using UnityEditor;
using UnityEngine;

namespace DialogueGraph {
    public class DlogGraphObject : ScriptableObject, ISerializationCallbackReceiver {
        [NonSerialized] private DlogGraphData m_GraphData;
        [NonSerialized] private int objectVersion;

        [SerializeField] public string AssetGuid;
        [SerializeField] public bool IsBlackboardVisible;
        [SerializeField] private string serializedGraph;
        [SerializeField] private int fileVersion;
        [SerializeField] private bool isDirty;

        public DlogGraphData GraphData {
            get => this.m_GraphData;
            private set {
                this.m_GraphData = value;
                if (this.m_GraphData != null)
                    this.m_GraphData.Owner = this;
            }
        }

        public void Initialize(DlogGraphData graphData) {
            this.GraphData = graphData;
            IsBlackboardVisible = this.m_GraphData.IsBlackboardVisible;
        }

        public bool IsDirty {
            get => isDirty;
            set => isDirty = value;
        }

        public bool WasUndoRedoPerformed => objectVersion != fileVersion;

        public void RegisterCompleteObjectUndo(string name) {
            Undo.RegisterCompleteObjectUndo(this, name);
            fileVersion++;
            objectVersion++;
            isDirty = true;
        }

        public void OnBeforeSerialize() {
            if(this.m_GraphData == null) return;

            serializedGraph = JsonUtility.ToJson(this.m_GraphData, true);
            AssetGuid = this.m_GraphData.AssetGuid;
        }

        public void OnAfterDeserialize() {
            if(this.m_GraphData != null) return;
            this.GraphData = Deserialize();
        }

        public void HandleUndoRedo() {
            if (!WasUndoRedoPerformed) {
                Debug.LogError("Trying to handle undo/redo when undo/redo was not performed", this);
                return;
            }

            DlogGraphData graphData = Deserialize();
            this.m_GraphData.ReplaceWith(graphData);
        }

        private DlogGraphData Deserialize() {
            DlogGraphData graphData = JsonUtility.FromJson<DlogGraphData>(serializedGraph);
            graphData.AssetGuid = AssetGuid;
            objectVersion = fileVersion;
            serializedGraph = "";
            return graphData;
        }

        public void RecalculateAssetGuid(string assetPath) {
            AssetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            this.m_GraphData.AssetGuid = AssetGuid;
        }
    }
}