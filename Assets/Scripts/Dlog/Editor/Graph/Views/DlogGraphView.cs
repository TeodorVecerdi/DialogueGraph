using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Dlog {
    public class DlogGraphView : GraphView {

        private EditorView editorView;

        public DlogGraphView(EditorView editorView) {
            this.editorView = editorView;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            // var mousePosition = evt.mousePosition;
            base.BuildContextualMenu(evt);
            evt.menu.AppendSeparator();
            if (evt.target is Node || evt.target is StickyNote) {
                // TODO: GROUP
                evt.menu.AppendAction("Group Selection %g", _ => { }, actionStatusCallback => DropdownMenuAction.Status.Disabled);

                // TODO: UNGROUP
                evt.menu.AppendAction("Ungroup Selection %u", _ => { }, actionStatusCallback => DropdownMenuAction.Status.Disabled);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port => {
                if (startPort != port && startPort.node != port.node && port.direction != startPort.direction) {
                    // TODO: Add type check for ports
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }
    }
}