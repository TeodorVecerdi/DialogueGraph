using System;
using UnityEngine;

namespace Dlog {
    [Serializable]
    public struct NodeDrawState {
        [SerializeField] public Rect Position;
        [SerializeField] public bool Expanded;
    }
}