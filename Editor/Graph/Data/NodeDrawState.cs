using System;
using UnityEngine;

namespace DialogueGraph {
    [Serializable]
    public struct NodeDrawState {
        [SerializeField] public Rect Position;
        [SerializeField] public bool Expanded;
    }
}