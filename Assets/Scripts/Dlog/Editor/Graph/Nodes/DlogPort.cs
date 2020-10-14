using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Dlog {
    public class DlogPort : Port {
        private DlogPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type) { }

        public static Port Create(string name, Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type, EdgeConnectorListener edgeConnectorListener) {
            var port = new DlogPort(portOrientation, portDirection, portCapacity, type);
            if (edgeConnectorListener != null) {
                port.m_EdgeConnector = new EdgeConnector<Edge>(edgeConnectorListener);
                port.AddManipulator(port.m_EdgeConnector);
            }

            port.viewDataKey = Guid.NewGuid().ToString();
            port.portName = name;
            return port;
        }
    }
}