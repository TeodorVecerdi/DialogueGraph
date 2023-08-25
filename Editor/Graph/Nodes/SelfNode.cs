using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueGraph {
    public class LineDataSelf {
        public string Line;
        public string PortGuidA;
        public string PortGuidB;
    }

    [Title("Self")]
    public class SelfNode : AbstractNode {
        private readonly List<LineDataSelf> m_Lines = new();
        private VisualElement m_LineLabel;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Self", EditorView.DefaultNodePosition);

            this.m_LineLabel = new Label { name = "lineTitle", text = "Lines" };
            outputContainer.Add(this.m_LineLabel);

            Button button = new(() => AddConversationPort(true)) { text = "Create Dialogue Line" };
            extensionContainer.Add(button);
            Label titleLabel = this.Q<Label>("title-label");
            VisualElement titleElement = this.Q("title");
            VisualElement titleC = UIElementsFactory.VisualElement<VisualElement>("title-container", null);
            titleLabel.RemoveFromHierarchy();
            titleC.Add(titleLabel);
            titleElement.Insert(0, titleC);

            DlogPort branchPort = DlogPort.Create("Branch", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, PortType.Branch, true, edgeConnectorListener);
            branchPort.AddToClassList("square-circle");
            titleC.Insert(0, branchPort);
            AddPort(branchPort, false);
            Refresh();
        }

        public override void SetNodeData(string jsonData) {
            if (string.IsNullOrEmpty(jsonData)) return;
            base.SetNodeData(jsonData);

            JObject data = JObject.Parse(jsonData);
            List<LineDataSelf> lines = JsonConvert.DeserializeObject<List<LineDataSelf>>(data.Value<string>("lines"));
            this.m_Lines.Clear();
            this.m_Lines.AddRange(lines);

            for (int i = 0; i < this.m_Lines.Count; i++) {
                this.AddConversationPort(false, i);
            }
        }

        public override string GetNodeData() {
            JObject root = new() {
                ["lines"] = new JValue(JsonConvert.SerializeObject(this.m_Lines)),
            };

            string baseNodeData = base.GetNodeData();
            if (baseNodeData != "{}") {
                root.Merge(JObject.Parse(baseNodeData));
            }

            return root.ToString(Formatting.None);
        }

        private void AddConversationPort(bool create, int index = -1) {
            VisualElement conversationContainer = new() { name = "conversation-container" };
            this.m_LineLabel.AddToClassList("visible");

            if (create) {
                Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Created Dialogue Line");
                index = this.m_Lines.Count;
                this.m_Lines.Add(new LineDataSelf { Line = "" });
            }

            TextField message = UIElementsFactory.TextField("conversation-item", "Line", new[] { "message" }, null, null, true);
            if (!create) {
                message.SetValueWithoutNotify(this.m_Lines[index].Line);
            }

            DlogPort branchPort = DlogPort.Create("Branch", Orientation.Horizontal, Direction.Output, Port.Capacity.Single, PortType.Branch, true, EdgeConnectorListener);
            branchPort.name = "conversation-item";
            branchPort.AddToClassList("branch-port");

            DlogPort triggerPort = DlogPort.Create("Trigger", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Trigger, false, EdgeConnectorListener);
            triggerPort.name = "conversation-item";
            triggerPort.AddToClassList("trigger-port");

            VisualElement flexBreak = UIElementsFactory.FlexBreaker();
            if (create) {
                this.m_Lines[index].PortGuidA = branchPort.viewDataKey;
                this.m_Lines[index].PortGuidB = triggerPort.viewDataKey;
            } else {
                branchPort.viewDataKey = this.m_Lines[index].PortGuidA;
                triggerPort.viewDataKey = this.m_Lines[index].PortGuidB;
            }

            message.RegisterCallback<FocusOutEvent>(_ => {
                int lineIndex = this.m_Lines.FindIndex(data => data.PortGuidA == branchPort.viewDataKey);
                if (message.value != this.m_Lines[lineIndex].Line) {
                    Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Changed Dialogue Line");
                    this.m_Lines[lineIndex].Line = message.value;
                }
            });

            Button removeButton = UIElementsFactory.Button(
                "x", "conversation-item", "Remove line", new[] { "remove-button" },
                () => this.RemoveLine(this.m_Lines.FindIndex(data => data.PortGuidA == branchPort.viewDataKey))
            );

            conversationContainer.Add(message);
            conversationContainer.Add(branchPort);
            conversationContainer.Add(flexBreak);
            conversationContainer.Add(removeButton);
            conversationContainer.Add(triggerPort);

            VisualElement separator = new() { name = "divider" };
            separator.AddToClassList("horizontal");
            separator.AddToClassList("horizontal-divider");
            outputContainer.Add(separator);

            outputContainer.Add(conversationContainer);
            Ports.Add(branchPort);
            Ports.Add(triggerPort);

            if (create) {
                Owner.PortData.Add(branchPort.viewDataKey);
                Owner.PortData.Add(triggerPort.viewDataKey);
                Owner.GuidPortDictionary.Add(branchPort.viewDataKey, branchPort);
                Owner.GuidPortDictionary.Add(triggerPort.viewDataKey, triggerPort);
            }

            Refresh();
        }

        private void RemoveLine(int index) {
            if (this.m_Lines.Count == 1) {
                this.m_LineLabel.RemoveFromClassList("visible");
            }

            VisualElement container = outputContainer.Children().Where(element => element.name == "conversation-container").ToList()[index];
            VisualElement separator = outputContainer.Children().Where(element => element.name == "divider" && element.ClassListContains("horizontal-divider")).ToList()[index];
            outputContainer.Remove(separator);

            this.Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Removed Line");

            DlogGraphData graphData = this.Owner.EditorView.DlogObject.GraphData;
            List<SerializedEdge> edgesToRemove = graphData.Edges.Where(edge => edge.InputPort == this.m_Lines[index].PortGuidA || edge.OutputPort == this.m_Lines[index].PortGuidA).ToList();
            edgesToRemove.AddRange(graphData.Edges.Where(edge => edge.InputPort == this.m_Lines[index].PortGuidB || edge.OutputPort == this.m_Lines[index].PortGuidB));

            graphData.RemoveElements(new List<SerializedNode>(), edgesToRemove);
            Owner.PortData.Remove(this.m_Lines[index].PortGuidA);
            Owner.PortData.Remove(this.m_Lines[index].PortGuidB);
            Port portA = Owner.GuidPortDictionary[this.m_Lines[index].PortGuidA];
            Port portB = Owner.GuidPortDictionary[this.m_Lines[index].PortGuidB];
            Owner.GuidPortDictionary.Remove(this.m_Lines[index].PortGuidA);
            Owner.GuidPortDictionary.Remove(this.m_Lines[index].PortGuidB);
            Ports.Remove(portA);
            Ports.Remove(portB);
            this.m_Lines.RemoveAt(index);
            outputContainer.Remove(container);

            Refresh();
        }
    }
}