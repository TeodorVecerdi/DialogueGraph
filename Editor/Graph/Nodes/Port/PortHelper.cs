using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public static class PortHelper {
        public static bool IsCompatibleWith(this DlogPort port, DlogPort other) {
            switch (port.Type) {
                case PortType.Check:
                    return other.Type == PortType.Check;
                case PortType.Trigger:
                    return other.Type == PortType.Trigger;
                case PortType.Actor:
                    return other.Type == PortType.Actor;
                case PortType.Branch:
                    return other.Type == PortType.Branch;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Color PortColor(DlogPort port) {
            switch (port.Type) {
                case PortType.Check:
                    if (port.direction == Direction.Input)
                        return new Color(0.2f, 0.73f, 1f);
                    return new Color(0.5f, 0.98f, 1f);
                case PortType.Trigger:
                    if (port.direction == Direction.Input)
                        return new Color(1f, 0.15f, 0.26f);
                    return new Color(0.84f, 0.26f, 0.16f);
                case PortType.Actor:
                    if (port.direction == Direction.Input)
                        return new Color(0.55f, 1f, 0.3f);
                    return new Color(0.75f, 1f, 0.36f);
                case PortType.Branch:
                    if (port.direction == Direction.Input)
                        return new Color(0.9f, 1f, 0.99f);
                    return new Color(0.91f, 0.93f, 1f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(port), port, "Undefined color for port type.");
            }
        }
    }
}