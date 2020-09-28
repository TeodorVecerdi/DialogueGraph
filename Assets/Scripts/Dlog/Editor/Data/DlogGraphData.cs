using System;
using UnityEngine;

namespace Dlog {
    [Serializable]
    public class DlogGraphData : ISerializationCallbackReceiver {
        public DlogGraphObject Owner { get; set; }
        [SerializeField] public string AssetGuid;
        [SerializeField] public bool IsBlackboardVisible;

        public void OnBeforeSerialize() {
            if(Owner != null)
                IsBlackboardVisible = Owner.IsBlackboardVisible;
        }

        public void OnAfterDeserialize() {
        }
    }
}