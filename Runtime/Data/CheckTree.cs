using System;
using UnityEngine;

namespace DialogueGraph.Runtime {
    [Serializable]
    public class CheckTree : ISerializationCallbackReceiver {
        public enum Kind {
            Property,
            Unary,
            Binary
        }

        public Kind NodeKind;
        public BooleanOperation BooleanOperation;

        // For NodeKind == Property, also if this is null and NodeKind == Property, then the property is assumed to be false
        public string PropertyGuid;

        // ForNodeKind == Unary and Binary
        [NonSerialized] public CheckTree SubtreeA;

        // For NodeKind == Binary
        [NonSerialized] public CheckTree SubtreeB;

        [SerializeField] private string m_SerializedSubtreeA;
        [SerializeField] private string m_SerializedSubtreeB;

        public static CheckTree Property(string propertyGuid) {
            return new CheckTree {
                NodeKind = Kind.Property,
                PropertyGuid = propertyGuid
            };
        }

        public static CheckTree Unary(BooleanOperation operation, CheckTree subtree) {
            return new CheckTree {
                NodeKind = Kind.Unary,
                BooleanOperation = operation,
                SubtreeA = subtree
            };
        }

        public static CheckTree Binary(BooleanOperation operation, CheckTree subtreeA, CheckTree subtreeB) {
            return new CheckTree {
                NodeKind = Kind.Binary,
                BooleanOperation = operation,
                SubtreeA = subtreeA,
                SubtreeB = subtreeB
            };
        }

        public void OnBeforeSerialize() {
            switch (NodeKind) {
                case Kind.Property:
                    m_SerializedSubtreeA = null;
                    m_SerializedSubtreeB = null;
                    break;
                case Kind.Unary:
                    m_SerializedSubtreeA = SubtreeA != null ? JsonUtility.ToJson(SubtreeA) : "";
                    m_SerializedSubtreeB = null;
                    break;
                case Kind.Binary:
                    m_SerializedSubtreeA = SubtreeA != null ? JsonUtility.ToJson(SubtreeA) : "";
                    m_SerializedSubtreeB = SubtreeB != null ? JsonUtility.ToJson(SubtreeB) : "";
                    break;
            }
        }

        public void OnAfterDeserialize() {
            switch (NodeKind) {
                case Kind.Property:
                    m_SerializedSubtreeA = null;
                    m_SerializedSubtreeB = null;
                    break;
                case Kind.Unary:
                    SubtreeA = m_SerializedSubtreeA != "" ? JsonUtility.FromJson<CheckTree>(m_SerializedSubtreeA) : null;
                    m_SerializedSubtreeB = null;
                    break;
                case Kind.Binary:
                    SubtreeA = m_SerializedSubtreeA != "" ? JsonUtility.FromJson<CheckTree>(m_SerializedSubtreeA) : null;
                    SubtreeB = m_SerializedSubtreeB != "" ? JsonUtility.FromJson<CheckTree>(m_SerializedSubtreeB) : null;
                    break;
            }
        }
    }
}