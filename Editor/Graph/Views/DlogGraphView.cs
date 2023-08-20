using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueGraph {
    public class DlogGraphView : GraphView {
        public EditorView EditorView { get; }

        public DlogGraphData DlogGraph => EditorView.DlogObject.GraphData;

        public DlogGraphView(EditorView editorView) {
            EditorView = editorView;
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerformed);
            serializeGraphElements = SerializeGraphElementsImpl;
            unserializeAndPaste = UnserializeAndPasteImpl;
            deleteSelection = DeleteSelectionImpl;
        }

        protected override bool canCopySelection => selection.OfType<AbstractNode>().Any() || selection.OfType<Group>().Any() || selection.OfType<BlackboardField>().Any();

        private void UnserializeAndPasteImpl(string operation, string data) {
            EditorView.DlogObject.RegisterCompleteObjectUndo(operation);
            var copyPasteData = CopyPasteData.FromJson(data);
            this.InsertCopyPasteData(copyPasteData);
        }

        private string SerializeGraphElementsImpl(IEnumerable<GraphElement> elements) {
            var elementsList = elements.ToList();
            var nodes = elementsList.OfType<AbstractNode>().Select(x => x.Owner);
            var edges = elementsList.OfType<Edge>().Select(x => x.userData).OfType<SerializedEdge>();
            var properties = selection.OfType<BlackboardField>().Select(x => x.userData as AbstractProperty);

            // Collect the property nodes and get the corresponding properties
            var propertyNodeGuids = nodes.OfType<PropertyNode>().Select(x => x.PropertyGuid);
            var metaProperties = EditorView.DlogObject.GraphData.Properties.Where(x => propertyNodeGuids.Contains(x.GUID));

            var copyPasteData = new CopyPasteData(EditorView, nodes, edges, properties, metaProperties);
            return JsonUtility.ToJson(copyPasteData, true);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            base.BuildContextualMenu(evt);
            evt.menu.AppendSeparator();
            if (evt.target is Node || evt.target is StickyNote) {
                // TODO: GROUP
                evt.menu.AppendAction("Group Selection %g", _ => { }, actionStatusCallback => DropdownMenuAction.Status.Disabled);

                // TODO: UNGROUP
                evt.menu.AppendAction("Ungroup Selection %u", _ => { }, actionStatusCallback => DropdownMenuAction.Status.Disabled);
            }

            if (evt.target is BlackboardField) {
                evt.menu.AppendAction("Delete", _ => { DeleteSelectionImpl("Delete", AskUser.DontAskUser); }, actionStatusCallback => canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port => {
                if (startPort != port /*&& startPort.node != port.node*/ && port.direction != startPort.direction && (startPort as DlogPort).IsCompatibleWith(port as DlogPort)) {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        private void DeleteSelectionImpl(string operation, AskUser askUser) {
            var nodesToDelete = selection.OfType<AbstractNode>().Select(node => node.Owner);
            EditorView.DlogObject.RegisterCompleteObjectUndo(operation);
            EditorView.DlogObject.GraphData.RemoveElements(nodesToDelete.ToList(),
                selection.OfType<Edge>().Select(e => e.userData).OfType<SerializedEdge>().ToList());
            
            
            foreach (var selectable in selection) {
                if (!(selectable is BlackboardField field) || field.userData == null)
                    continue;
                var property = (AbstractProperty) field.userData;
                EditorView.DlogObject.GraphData.RemoveProperty(property);
            }
            
            selection.Clear();
        }

        #region Drag & Drop
        private void OnDragUpdated(DragUpdatedEvent evt) {
            var selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            var dragging = false;
            if (selection != null)
                if (selection.OfType<BlackboardField>().Any())
                    dragging = true;

            if (dragging) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }
        }

        private void OnDragPerformed(DragPerformEvent evt) {
            Vector2 localPos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);

            var selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            if (selection != null) {
                // Blackboard
                if (selection.OfType<BlackboardField>().Any()) {
                    var fields = selection.OfType<BlackboardField>();
                    foreach (var field in fields) {
                        CreateNode(field, localPos);
                    }
                }
            }
        }

        private void CreateNode(object obj, Vector2 nodePosition) {
            if (obj is BlackboardField blackboardField) {
                EditorView.DlogObject.RegisterCompleteObjectUndo("Drag Blackboard Field");
                var property = blackboardField.userData as AbstractProperty;
                var node = new SerializedNode(typeof(PropertyNode), new Rect(nodePosition, EditorView.DefaultNodeSize));
                EditorView.DlogObject.GraphData.AddNode(node);
                node.BuildNode(EditorView, EditorView.EdgeConnectorListener, false);
                var propertyNode = node.Node as PropertyNode;
                propertyNode.PropertyGuid = property.GUID;
                node.BuildPortData();
            }
        }
        #endregion
    }
}