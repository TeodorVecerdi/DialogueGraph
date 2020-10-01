using System;
using UnityEditor;
using UnityEngine;

namespace Dlog {
    public class DlogGraphObject : ScriptableObject, ISerializationCallbackReceiver {
        [NonSerialized] private DlogGraphData dlogGraph;
        [NonSerialized] private int objectVersion;
        
        [SerializeField] public string AssetGuid;
        [SerializeField] public bool IsBlackboardVisible;
        [SerializeField] private string serializedGraph;
        [SerializeField] private int fileVersion;
        [SerializeField] private bool isDirty;

        public DlogGraphData DlogGraph {
            get => dlogGraph;
            set {
                dlogGraph = value;
                if (dlogGraph != null)
                    dlogGraph.Owner = this;
            }
        }

        public void Initialize(DlogGraphData dlogData) {
            DlogGraph = dlogData;
            IsBlackboardVisible = DlogGraph.IsBlackboardVisible;
        }

        public bool IsDirty {
            get => isDirty;
            set => isDirty = value;
        }

        public bool WasUndoRedoPerformed => objectVersion != fileVersion;

        public void RegisterCompleteObjectUndo(string name) {
            Debug.Log($"Registered complete object undo with data:\n{dlogGraph.DebugString()}");
            Undo.RegisterCompleteObjectUndo(this, name);
            fileVersion++;
            objectVersion++;
            isDirty = true;
        }

        public void OnBeforeSerialize() {
            if(dlogGraph == null) return;

            serializedGraph = JsonUtility.ToJson(dlogGraph, true);
            AssetGuid = dlogGraph.AssetGuid;
        }

        public void OnAfterDeserialize() {
            if(DlogGraph != null) return;
            DlogGraph = Deserialize();
        }

        public void HandleUndoRedo() {
            if (!WasUndoRedoPerformed) {
                Debug.LogError("Trying to handle undo/redo when undo/redo was not performed", this);
                return;
            }

            var deserialized = Deserialize();
            dlogGraph.ReplaceWith(deserialized);
        }

        private DlogGraphData Deserialize() {
            var deserialized = JsonUtility.FromJson<DlogGraphData>(serializedGraph);
            deserialized.AssetGuid = AssetGuid;
            objectVersion = fileVersion;
            serializedGraph = "";
            return deserialized;
        }

        public void RecalculateAssetGuid(string assetPath) {
            AssetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            dlogGraph.AssetGuid = AssetGuid;
        }
    }
}