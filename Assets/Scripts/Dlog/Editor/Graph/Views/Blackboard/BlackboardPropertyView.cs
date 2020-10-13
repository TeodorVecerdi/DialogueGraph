using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public class BlackboardPropertyView : VisualElement {
        private readonly BlackboardField field;
        private readonly EditorView editorView;
        private AbstractProperty property;

        private TextField referenceNameField;
        private List<VisualElement> rows;
        public List<VisualElement> Rows => rows;

        private int undoGroup = -1;
        public int UndoGroup => undoGroup;

        private static Type contextualMenuManipulator = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).FirstOrDefault(t => t.FullName == "UnityEngine.UIElements.ContextualMenuManipulator");
        private IManipulator resetReferenceMenu;

        private EventCallback<KeyDownEvent> keyDownCallback;
        public EventCallback<KeyDownEvent> KeyDownCallback => keyDownCallback;
        private EventCallback<FocusOutEvent> focusOutCallback;
        public EventCallback<FocusOutEvent> FocusOutCallback => focusOutCallback;

        public BlackboardPropertyView(BlackboardField field, EditorView editorView, AbstractProperty property) {
            this.AddStyleSheet("Styles/PropertyView/Blackboard");
            this.field = field;
            this.editorView = editorView;
            this.property = property;
            rows = new List<VisualElement>();

            keyDownCallback = evt => {
                // Record Undo for input field edit
                if (undoGroup == -1) {
                    undoGroup = Undo.GetCurrentGroup();
                    editorView.DlogObject.RegisterCompleteObjectUndo("Change property value");
                }

                // Handle escaping input field edit
                if (evt.keyCode == KeyCode.Escape && undoGroup > -1) {
                    Undo.RevertAllDownToGroup(undoGroup);
                    undoGroup = -1;
                    evt.StopPropagation();
                }

                // Dont record Undo again until input field is unfocused
                undoGroup++;
                MarkDirtyRepaint();
            };

            focusOutCallback = evt => undoGroup = -1;

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

            if (property.Type == PropertyType.Actor) {
                var actorProperty = property as ActorProperty;
                var actorName = actorProperty.Value.Name;
                if (string.IsNullOrEmpty(actorName)) actorName = "New Actor";
                
                var nameField = new TextField(512, false, false, ' ') {value = actorName};
                
                nameField.RegisterValueChangedCallback(evt => {
                    editorView.DlogObject.RegisterCompleteObjectUndo("Change actor name");
                    var actorPropertyValue = actorProperty.Value;
                    actorPropertyValue.Name = evt.newValue;
                    actorProperty.Value = actorPropertyValue;
                    MarkDirtyRepaint();
                });
                
                AddRow("Actor Name", nameField);
            }
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
            rows.Add(rowView);
            return rowView;
        }

        public void Rebuild() {
            rows.Where(t => t.parent == this).ToList().ForEach(Remove);
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