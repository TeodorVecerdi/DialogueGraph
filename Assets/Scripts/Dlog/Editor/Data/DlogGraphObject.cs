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

        public void OnBeforeSerialize() {
            if(dlogGraph == null) return;

            serializedGraph = JsonUtility.ToJson(dlogGraph, true);
            AssetGuid = dlogGraph.AssetGuid;
        }

        public void OnAfterDeserialize() {
            if(dlogGraph != null) return;

            var deserialized = JsonUtility.FromJson<DlogGraphData>(serializedGraph);
            deserialized.AssetGuid = AssetGuid;
            objectVersion = fileVersion;
            serializedGraph = "";
            dlogGraph = deserialized;
        }

        public void RegisterCompleteObjectUndo(string name) {
            Undo.RegisterCompleteObjectUndo(this, name);
            fileVersion++;
            objectVersion++;
            isDirty = true;
        }

        public void RecalculateAssetGuid(string assetPath) {
            AssetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            dlogGraph.AssetGuid = AssetGuid;
        }
    }
}