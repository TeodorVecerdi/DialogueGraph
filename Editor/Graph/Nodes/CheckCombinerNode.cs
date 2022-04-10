using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueGraph {
    [Title("Combine Checks (AND)"), Title("Combine Checks (OR)")]
    public class CheckCombinerNode : AbstractNode {
        private bool operation;
        public bool Operation {
            get => operation;
            set {
                operation = value;
                var label = this.Q<Label>("combiner-operation");
                label.text = value ? "OR" : "AND";
                var main = this.Q("combiner-main");
                main.RemoveFromClassList(value ? "AND" : "OR");
                main.AddToClassList(value ? "OR" : "AND");
            }
        }

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Combiner", EditorView.DefaultNodePosition);
            this.AddStyleSheet("Styles/Node/CombinerNode");

            var inA = DlogPort.Create("A", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Boolean, true, edgeConnectorListener, true);
            var inB = DlogPort.Create("B", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Boolean, true, edgeConnectorListener, true);
            var @out = DlogPort.Create("OUT", Orientation.Horizontal, Direction.Output, Port.Capacity.Single, PortType.Boolean, true, edgeConnectorListener, true);

            AddPort(inA, false);
            AddPort(inB, false);
            AddPort(@out, false);

            var combinerMain = UIElementsFactory.VisualElement<VisualElement>("combiner-main", null);
            var combinerInput = UIElementsFactory.VisualElement<VisualElement>("combiner-input", new[] {"combiner-column"});
            var combinerOperation = UIElementsFactory.TextElement<Label>("combiner-operation", "AND", new[] {"combiner-column"});
            var combinerOutput = UIElementsFactory.VisualElement<VisualElement>("combiner-output", new[] {"combiner-column"});

            combinerMain.Add(combinerInput);
            combinerMain.Add(combinerOperation);
            combinerMain.Add(combinerOutput);
            combinerInput.Add(inA);
            combinerInput.Add(inB);
            combinerOutput.Add(@out);

            combinerMain.RegisterCallback<PointerDownEvent>(evt => {
                if (evt.clickCount == 2 && evt.button == 0) {
                    ChangeOperation(true);
                }
            });

            titleContainer.Clear();
            titleContainer.Add(combinerMain);
            Refresh();
        }

        private void ChangeOperation(bool registerUndo) {
            if (registerUndo) Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Change Combiner Operation");
            Operation = !Operation;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            base.BuildContextualMenu(evt);
            evt.menu.AppendAction("Change Operation", action => { ChangeOperation(true); });
            evt.menu.AppendSeparator();
        }

        public override string GetNodeData() {
            var root = new JObject();
            root["operation"] = operation;
            root.Merge(JObject.Parse(base.GetNodeData()));
            return root.ToString(Formatting.None);
        }

        public override void SetNodeData(string jsonData) {
            if (string.IsNullOrEmpty(jsonData)) return;
            base.SetNodeData(jsonData);
            var root = JObject.Parse(jsonData);
            Operation = root.Value<bool>("operation");
        }
    }
}