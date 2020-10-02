using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public class BlackboardProvider {
        private static readonly Texture2D exposedIcon = Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed");
        public Blackboard Blackboard { get; private set; }
        private readonly DlogGraphView graphView;
        private readonly Dictionary<string, BlackboardRow> inputRows;
        private readonly BlackboardSection sectionA;
        private readonly BlackboardSection sectionB;
        private List<Node> selectedNodes = new List<Node>();
        private Dictionary<string, bool> expandedInputs = new Dictionary<string, bool>();

        public Dictionary<string, bool> ExpandedInputs => expandedInputs;
        public string AssetName {
            get => Blackboard.title;
            set => Blackboard.title = value;
        }

        public BlackboardProvider(DlogGraphView graphView) {
            this.graphView = graphView;
            inputRows = new Dictionary<string, BlackboardRow>();

            Blackboard = new Blackboard {
                scrollable = true,
                title = "Events and Properties",
                subTitle = "Dialogue Properties",
                editTextRequested = EditTextRequested,
                addItemRequested = AddItemRequested,
                moveItemRequested = MoveItemRequested
            };

            sectionA = new BlackboardSection {title = "Test Section (A)"};
            Blackboard.Add(sectionA);
            sectionB = new BlackboardSection {title = "Test Section (B)"};
            Blackboard.Add(sectionB);
        }

        private void EditTextRequested(Blackboard blackboard, VisualElement visualElement, string newText) {
            var field = (BlackboardField) visualElement;
            if (!string.IsNullOrEmpty(newText)) {
                field.text = newText;
            }
        }

        private void MoveItemRequested(Blackboard blackboard, int newIndex, VisualElement visualElement) { }

        private void AddItemRequested(Blackboard blackboard) {
            var menu = new GenericMenu();
            AddSectionAItems(menu);
            AddSectionBItems(menu);
            menu.ShowAsContext();
        }

        private void AddSectionAItems(GenericMenu menu) {
            menu.AddItem(new GUIContent($"Section A/Vector1"), false, () => AddInputRow("Vector1", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Vector2"), false, () => AddInputRow("Vector2", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Vector3"), false, () => AddInputRow("Vector3", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Vector4"), false, () => AddInputRow("Vector4", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Color"), false, () => AddInputRow("Color", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Texture2D"), false, () => AddInputRow("Texture2D", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Texture2D Array"), false, () => AddInputRow("Texture2DArray", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Texture3D"), false, () => AddInputRow("Texture3D", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Cubemap"), false, () => AddInputRow("Cubemap", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Boolean"), false, () => AddInputRow("Boolean", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Matrix2x2"), false, () => AddInputRow("Matrix2", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Matrix3x3"), false, () => AddInputRow("Matrix3", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Matrix4x4"), false, () => AddInputRow("Matrix4", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/SamplerState"), false, () => AddInputRow("SamplerState", sectionA, true));
            menu.AddItem(new GUIContent($"Section A/Gradient"), false, () => AddInputRow("Gradient", sectionA, true));
            menu.AddSeparator($"/");
        }

        private void AddSectionBItems(GenericMenu menu) {
            menu.AddItem(new GUIContent($"Section B/Vector1"), false, () => AddInputRow("Vector1", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Vector2"), false, () => AddInputRow("Vector2", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Vector3"), false, () => AddInputRow("Vector3", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Vector4"), false, () => AddInputRow("Vector4", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Color"), false, () => AddInputRow("Color", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Texture2D"), false, () => AddInputRow("Texture2D", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Texture2D Array"), false, () => AddInputRow("Texture2DArray", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Texture3D"), false, () => AddInputRow("Texture3D", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Cubemap"), false, () => AddInputRow("Cubemap", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Boolean"), false, () => AddInputRow("Boolean", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Matrix2x2"), false, () => AddInputRow("Matrix2", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Matrix3x3"), false, () => AddInputRow("Matrix3", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Matrix4x4"), false, () => AddInputRow("Matrix4", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/SamplerState"), false, () => AddInputRow("SamplerState", sectionB, true));
            menu.AddItem(new GUIContent($"Section B/Gradient"), false, () => AddInputRow("Gradient", sectionB, true));
            menu.AddSeparator($"/");
        }

        private void AddInputRow(string input, BlackboardSection section, bool create = false, int index = -1) {
            if (inputRows.ContainsKey(input))
                return;

            BlackboardField field = new BlackboardField(exposedIcon, input.ToUpper(), input);
            BlackboardRow row = new BlackboardRow(field, new Label(input + " (Property Field)")) {userData = input};
            if (index < 0)
                index = inputRows.Count;
            if (index == inputRows.Count)
                section.Add(row);
            else
                section.Insert(index, row);

            var pill = row.Q<Pill>();
            pill.RegisterCallback<MouseEnterEvent>(evt => OnMouseHover(evt, input));
            pill.RegisterCallback<MouseLeaveEvent>(evt => OnMouseHover(evt, input));
            pill.RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);

            var expandButton = row.Q<Button>("expandButton");
            expandButton.RegisterCallback<MouseDownEvent>(evt => OnExpanded(evt, input), TrickleDown.TrickleDown);

            inputRows[input] = row;

            row.expanded = true;
            expandedInputs[input] = true;
            field.OpenTextEditor();
        }

        private void OnExpanded(MouseDownEvent evt, string input) {
            expandedInputs[input] = !inputRows[input].expanded;
        }

        private void OnMouseHover(EventBase evt, string input) {
            if (evt.eventTypeId == MouseEnterEvent.TypeId()) {
                foreach (var node in graphView.nodes.ToList()) {
                    if (node.viewDataKey == input) {
                        selectedNodes.Add(node);
                        node.AddToClassList("hovered");
                    }
                }
            } else if (evt.eventTypeId == MouseLeaveEvent.TypeId() && selectedNodes.Any()) {
                foreach (var node in selectedNodes) {
                    node.RemoveFromClassList("hovered");
                }

                selectedNodes.Clear();
            }
        }

        private void OnDragUpdatedEvent(DragUpdatedEvent evt) {
            if (selectedNodes.Any()) {
                foreach (var node in selectedNodes) {
                    node.RemoveFromClassList("hovered");
                }
                selectedNodes.Clear();
            }
        }
    }
}