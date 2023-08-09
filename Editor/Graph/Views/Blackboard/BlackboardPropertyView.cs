using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DialogueGraph.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueGraph {
    public class BlackboardPropertyView : VisualElement {
        private readonly BlackboardField field;
        private readonly EditorView editorView;
        private AbstractProperty property;

        private TextField referenceNameField;
        public List<VisualElement> Rows { get; }

        public int UndoGroup { get; private set; } = -1;

        private static Type contextualMenuManipulator = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).FirstOrDefault(t => t.FullName == "UnityEngine.UIElements.ContextualMenuManipulator");
        private IManipulator resetReferenceMenu;

        public EventCallback<KeyDownEvent> KeyDownCallback { get; }

        public EventCallback<FocusOutEvent> FocusOutCallback { get; }

        public BlackboardPropertyView(BlackboardField field, EditorView editorView, AbstractProperty property) {
            this.AddStyleSheet("Styles/PropertyView/Blackboard");
            this.field = field;
            this.editorView = editorView;
            this.property = property;
            Rows = new List<VisualElement>();

            KeyDownCallback = evt => {
                // Record Undo for input field edit
                if (UndoGroup == -1) {
                    UndoGroup = Undo.GetCurrentGroup();
                    editorView.DlogObject.RegisterCompleteObjectUndo("Change property value");
                }

                // Handle escaping input field edit
                if (evt.keyCode == KeyCode.Escape && UndoGroup > -1) {
                    Undo.RevertAllDownToGroup(UndoGroup);
                    UndoGroup = -1;
                    evt.StopPropagation();
                }

                // Dont record Undo again until input field is unfocused
                UndoGroup++;
                MarkDirtyRepaint();
            };

            FocusOutCallback = evt => UndoGroup = -1;

            BuildFields(property);
            AddToClassList("blackboardPropertyView");
        }

        private void BuildFields(AbstractProperty property) {
            referenceNameField = new TextField(512, false, false, ' ') {isDelayed = true, value = property.ReferenceName};
            referenceNameField.AddStyleSheet("Styles/PropertyView/ReferenceNameField");
            referenceNameField.RegisterValueChangedCallback(evt => {
                editorView.DlogObject.RegisterCompleteObjectUndo("Change Reference Name");
                editorView.DlogObject.DlogGraph.SanitizePropertyReference(property, evt.newValue);
                referenceNameField.value = property.ReferenceName;
                if (string.IsNullOrEmpty(property.OverrideReferenceName))
                    referenceNameField.RemoveFromClassList("modified");
                else
                    referenceNameField.AddToClassList("modified");

                Rebuild();
                UpdateReferenceNameResetMenu();
            });
            if (!string.IsNullOrEmpty(property.OverrideReferenceName))
                referenceNameField.AddToClassList("modified");

            AddRow("Reference Name", referenceNameField);
        }

        private void UpdateReferenceNameResetMenu() {
            if (string.IsNullOrEmpty(property.OverrideReferenceName)) {
                this.RemoveManipulator(resetReferenceMenu);
                resetReferenceMenu = null;
            } else {
                resetReferenceMenu = (IManipulator) Activator.CreateInstance(contextualMenuManipulator, (Action<ContextualMenuPopulateEvent>) BuildContextualMenu);
                this.AddManipulator(resetReferenceMenu);
            }
        }

        private void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            evt.menu.AppendAction("Reset Reference", e => {
                property.OverrideReferenceName = null;
                referenceNameField.value = property.ReferenceName;
                referenceNameField.RemoveFromClassList("modified");
            }, DropdownMenuAction.AlwaysEnabled);
        }

        public VisualElement AddRow(string labelText, VisualElement control, bool enabled = true) {
            var rowView = CreateRow(labelText, control, enabled);
            Add(rowView);
            Rows.Add(rowView);
            return rowView;
        }

        public void Rebuild() {
            Rows.Where(t => t.parent == this).ToList().ForEach(Remove);
            BuildFields(property);
        }

        private VisualElement CreateRow(string labelText, VisualElement control, bool enabled) {
            var rowView = new VisualElement();
            rowView.AddToClassList("rowView");
            if (!string.IsNullOrEmpty(labelText)) {
                var label = new Label(labelText);
                label.SetEnabled(enabled);
                label.AddToClassList("rowViewLabel");
                rowView.Add(label);
            }

            control.AddToClassList("rowViewControl");
            control.SetEnabled(enabled);

            rowView.Add(control);
            return rowView;
        }
    }
}