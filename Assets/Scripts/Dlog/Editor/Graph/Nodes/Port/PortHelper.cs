using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Dlog {
    public static class PortHelper {
        public static bool IsCompatibleWith(this DlogPort port, DlogPort other) {
            switch (port.PortType) {
                case PortType.Check:
                    return other.PortType == PortType.Check;
                case PortType.Trigger:
                    return other.PortType == PortType.Trigger;
                case PortType.Actor:
                    return other.PortType == PortType.Actor;
                case PortType.Branch:
                    return other.PortType == PortType.Branch;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Color PortColor(DlogPort port) {
            switch (port.PortType) {
                case PortType.Check:
                    if(port.direction == Direction.Input)
                        return new Color(0.2f, 0.73f, 1f);
                    return new Color(0.5f, 0.98f, 1f);
                case PortType.Trigger:
                    if(port.direction == Direction.Input)
                        return new Color(0.84f, 0.16f, 0.24f);
                    return new Color(0.84f, 0.33f, 0.22f);
                case PortType.Actor:
                    if(port.direction == Direction.Input)
                        return new Color(0.55f, 1f, 0.3f);
                    return new Color(0.75f, 1f, 0.36f);
                case PortType.Branch:
                    if(port.direction == Direction.Input)
                        return new Color(0.56f, 0.16f, 0.91f);
                    return new Color(0.67f, 0.24f, 0.91f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(port), port, "Undefined color for port type.");
            }
        }
    }
}