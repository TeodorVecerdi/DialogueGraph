using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueGraph {
    public static class PortHelper {
        public static bool IsCompatibleWith(this DlogPort port, DlogPort other) {
            return port.Type switch {
                PortType.Check => other.Type is PortType.Check or PortType.Boolean,
                PortType.Trigger => other.Type is PortType.Trigger,
                PortType.Actor => other.Type is PortType.Actor,
                PortType.Branch => other.Type is PortType.Branch,
                PortType.Boolean => other.Type is PortType.Check or PortType.Boolean,
                _ => throw new ArgumentOutOfRangeException(nameof(port), port, $"Undefined compatibility for port of type {port.Type.ToString()}."),
            };
        }

        public static Color PortColor(DlogPort port) {
            return port.Type switch {
                PortType.Check
                    => port.direction is Direction.Input ? new Color(0.2f, 0.73f, 1.0f) : new Color(0.5f, 0.98f, 1.0f),
                PortType.Trigger
                    => port.direction is Direction.Input ? new Color(1f, 0.15f, 0.26f) : new Color(0.84f, 0.26f, 0.16f),
                PortType.Actor
                    => port.direction is Direction.Input ? new Color(0.55f, 1.0f, 0.3f) : new Color(0.75f, 1.0f, 0.36f),
                PortType.Branch
                    => port.direction is Direction.Input ? new Color(0.9f, 1.0f, 0.99f) : new Color(0.91f, 0.93f, 1.0f),
                PortType.Boolean => new Color(0.45f, 0.25f, 1.0f),
                _ => throw new ArgumentOutOfRangeException(nameof(port), port, $"Undefined color for port of type {port.Type.ToString()}."),
            };
        }
    }
}