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
            get => m_GraphData;
            private set {
                m_GraphData = value;
                if (m_GraphData != null) {
                    m_GraphData.Owner = this;
                }
            }
        }

        public void Initialize(DlogGraphData graphData) {
            GraphData = graphData;
            IsBlackboardVisible = m_GraphData.IsBlackboardVisible;
        }

        public bool IsDirty {
            get => isDirty;
            set => isDirty = value;
        }

        public bool WasUndoRedoPerformed => m_ObjectVersion != fileVersion;

        public void RegisterCompleteObjectUndo(string operationName) {
            Undo.RegisterCompleteObjectUndo(this, operationName);
            fileVersion++;
            m_ObjectVersion++;
            isDirty = true;
        }

        public void OnBeforeSerialize() {
            if(m_GraphData == null) return;

            serializedGraph = JsonUtility.ToJson(m_GraphData, true);
            AssetGuid = m_GraphData.AssetGuid;
        }

        public void OnAfterDeserialize() {
            if(m_GraphData != null) return;
            GraphData = Deserialize();
        }

        public void HandleUndoRedo() {
            if (!WasUndoRedoPerformed) {
                Debug.LogError("Trying to handle undo/redo when undo/redo was not performed", this);
                return;
            }

            DlogGraphData graphData = Deserialize();
            m_GraphData.ReplaceWith(graphData);
        }

        private DlogGraphData Deserialize() {
            DlogGraphData graphData = JsonUtility.FromJson<DlogGraphData>(serializedGraph);
            graphData.AssetGuid = AssetGuid;
            m_ObjectVersion = fileVersion;
            serializedGraph = "";
            return graphData;
        }

        public void RecalculateAssetGuid(string assetPath) {
            AssetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            m_GraphData.AssetGuid = AssetGuid;
        }
    }
}