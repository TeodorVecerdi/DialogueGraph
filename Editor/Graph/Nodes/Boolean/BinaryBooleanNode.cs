using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueGraph {
    public abstract class BinaryBooleanNode : AbstractNode {
        protected abstract string Title { get; }
        protected abstract BooleanOperation Operation { get; }

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize(Title, EditorView.DefaultNodePosition);
            this.AddStyleSheet("Styles/Node/BooleanNode");
            AddToClassList(Operation.ToString());

            DlogPort inA = DlogPort.Create("INA", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Boolean, true, edgeConnectorListener, true);
            DlogPort inB = DlogPort.Create("INB", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Boolean, true, edgeConnectorListener, true);
            DlogPort @out = DlogPort.Create("OUT", Orientation.Horizontal, Direction.Output, Port.Capacity.Single, PortType.Boolean, true, edgeConnectorListener, true);

            AddPort(inA, false);
            AddPort(inB, false);
            AddPort(@out, false);

            VisualElement booleanMain = UIElementsFactory.VisualElement<VisualElement>("boolean-main", null);
            VisualElement booleanInput = UIElementsFactory.VisualElement<VisualElement>("boolean-input", new[] {"boolean-column"});
            VisualElement booleanOutput = UIElementsFactory.VisualElement<VisualElement>("boolean-output", new[] {"boolean-column"});

            booleanMain.Add(booleanInput);
            booleanMain.Add(UIElementsFactory.TextElement<Label>("boolean-operation", Operation.ToString(), new[] {"boolean-column"}));
            booleanMain.Add(booleanOutput);
            booleanInput.Add(inA);
            booleanInput.Add(inB);
            booleanOutput.Add(@out);

            titleContainer.Clear();
            titleContainer.Add(booleanMain);
            Refresh();
        }
    }
}