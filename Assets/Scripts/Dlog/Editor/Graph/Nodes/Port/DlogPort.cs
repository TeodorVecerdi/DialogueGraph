using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public class DlogPort : Port {
        private PortType portType;
        public PortType PortType => portType;
        private DlogPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity) : base(portOrientation, portDirection, portCapacity, typeof(object)) { }

        public static DlogPort Create(string name, Orientation portOrientation, Direction portDirection, Capacity portCapacity, PortType type, EdgeConnectorListener edgeConnectorListener) {
            var port = new DlogPort(portOrientation, portDirection, portCapacity);
            if (edgeConnectorListener != null) {
                port.m_EdgeConnector = new EdgeConnector<Edge>(edgeConnectorListener);
                port.AddManipulator(port.m_EdgeConnector);
            }

            port.portType = type;
            port.portColor = PortHelper.PortColor(port);
            port.viewDataKey = Guid.NewGuid().ToString();
            port.portName = name;
            return port;
        }
    }
}