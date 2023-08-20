using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueGraph {
    public class LineDataNpc {
        public string Line;
        public string PortGuidA;
        public string PortGuidB;
        public string PortGuidC;
    }

    [Title("NPC")]
    public class NpcNode : AbstractNode {
        private readonly List<LineDataNpc> m_Lines = new();
        private VisualElement m_LineLabel;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("NPC", EditorView.DefaultNodePosition);

            Button button = new(() => AddConversationPort(true)) {text = "Create Dialogue Line"};
            extensionContainer.Add(button);

            Label titleLabel = this.Q<Label>("title-label");
            VisualElement titleElement = this.Q("title");
            VisualElement titleC = UIElementsFactory.VisualElement<VisualElement>("title-container", null);
            titleLabel.RemoveFromHierarchy();
            titleC.Add(titleLabel);
            titleElement.Insert(0, titleC);

            m_LineLabel = new Label {name = "lineTitle", text = "Lines"};
            outputContainer.Add(m_LineLabel);
            VisualElement titlePortContainer = UIElementsFactory.VisualElement<VisualElement>("npc-title-port-container", null);

            DlogPort branchPort = DlogPort.Create("Branch", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, PortType.Branch, true, edgeConnectorListener);
            DlogPort actorPort = DlogPort.Create("Actor", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Actor, true, edgeConnectorListener);
            titlePortContainer.Add(branchPort);
            titlePortContainer.Add(actorPort);
            titleC.Insert(0, titlePortContainer);

            AddPort(branchPort, false);
            AddPort(actorPort, false);
            Refresh();
        }

        public override void SetNodeData(string jsonData) {
            if (string.IsNullOrEmpty(jsonData)) return;
            base.SetNodeData(jsonData);

            JObject data = JObject.Parse(jsonData);
            List<LineDataNpc> lines = JsonConvert.DeserializeObject<List<LineDataNpc>>(data.Value<string>("lines"));
            m_Lines.Clear();
            m_Lines.AddRange(lines);

            for (int i = 0; i < m_Lines.Count; i++) {
                AddConversationPort(false, i);
            }
        }

        public override string GetNodeData() {
            JObject root = new() {
                ["lines"] = new JValue(JsonConvert.SerializeObject(m_Lines)),
            };

            string baseNodeData = base.GetNodeData();
            if (baseNodeData != "{}") {
                root.Merge(JObject.Parse(baseNodeData));
            }

            return root.ToString(Formatting.None);
        }

        private void AddConversationPort(bool create, int index = -1) {
            m_LineLabel.AddToClassList("visible");
            VisualElement conversationContainer = new() {name = "conversation-container"};

            if (create) {
                Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Created Dialogue Line");
                index = m_Lines.Count;
                m_Lines.Add(new LineDataNpc {Line = ""});
            }

            TextField message = UIElementsFactory.TextField("conversation-item", "Line", new[] {"message"}, null, null, true);
            if (!create) {
                message.SetValueWithoutNotify(m_Lines[index].Line);
            }

            DlogPort branchPort = DlogPort.Create("Branch", Orientation.Horizontal, Direction.Output, Port.Capacity.Single, PortType.Branch, true, EdgeConnectorListener);
            branchPort.name = "conversation-item";
            branchPort.AddToClassList("branch-port");
            DlogPort triggerPort = DlogPort.Create("Trigger", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Trigger, false, EdgeConnectorListener);
            triggerPort.name = "conversation-item";
            triggerPort.AddToClassList("trigger-port");
            DlogPort checkPort = DlogPort.Create("Check", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, PortType.Check , false,EdgeConnectorListener);
            checkPort.name = "conversation-item";
            checkPort.AddToClassList("check-port");

            VisualElement flexBreak = UIElementsFactory.FlexBreaker();
            if (create) {
                m_Lines[index].PortGuidA = branchPort.viewDataKey;
                m_Lines[index].PortGuidB = triggerPort.viewDataKey;
                m_Lines[index].PortGuidC = checkPort.viewDataKey;
            } else {
                branchPort.viewDataKey = m_Lines[index].PortGuidA;
                triggerPort.viewDataKey = m_Lines[index].PortGuidB;
                checkPort.viewDataKey = m_Lines[index].PortGuidC;
            }

            message.RegisterCallback<FocusOutEvent>(evt => {
                int lineIndex = m_Lines.FindIndex(data => data.PortGuidA == branchPort.viewDataKey);
                if (message.value != m_Lines[lineIndex].Line) {
                    Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Changed Dialogue Line");
                    m_Lines[lineIndex].Line = message.value;
                }
            });
            Button removeButton = UIElementsFactory.Button("x", "conversation-item", "Remove line", new[] {"remove-button"}, () => { RemoveLine(m_Lines.FindIndex(data => data.PortGuidA == branchPort.viewDataKey)); });

            conversationContainer.Add(message);
            conversationContainer.Add(branchPort);
            conversationContainer.Add(flexBreak);
            conversationContainer.Add(checkPort);
            conversationContainer.Add(removeButton);
            conversationContainer.Add(triggerPort);

            VisualElement separator = new() {name = "divider"};
            separator.AddToClassList("horizontal");
            separator.AddToClassList("horizontal-divider");
            outputContainer.Add(separator);

            outputContainer.Add(conversationContainer);
            Ports.Add(branchPort);
            Ports.Add(triggerPort);
            Ports.Add(checkPort);
            if (create) {
                Owner.PortData.Add(branchPort.viewDataKey);
                Owner.PortData.Add(triggerPort.viewDataKey);
                Owner.PortData.Add(checkPort.viewDataKey);
                Owner.GuidPortDictionary.Add(branchPort.viewDataKey, branchPort);
                Owner.GuidPortDictionary.Add(triggerPort.viewDataKey, triggerPort);
                Owner.GuidPortDictionary.Add(checkPort.viewDataKey, checkPort);
            }

            Refresh();
        }

        private void RemoveLine(int index) {
            if (m_Lines.Count == 1)
                m_LineLabel.RemoveFromClassList("visible");

            VisualElement container = outputContainer.Children().Where(element => element.name == "conversation-container").ToList()[index];
            VisualElement separator = outputContainer.Children().Where(element => element.name == "divider" && element.ClassListContains("horizontal-divider")).ToList()[index];
            outputContainer.Remove(separator);

            Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Removed Line");
            List<SerializedEdge> edgesToRemove = Owner.EditorView.DlogObject.GraphData.Edges.Where(edge => edge.InputPort == m_Lines[index].PortGuidA || edge.OutputPort == m_Lines[index].PortGuidA).ToList();
            edgesToRemove.AddRange(Owner.EditorView.DlogObject.GraphData.Edges.Where(edge => edge.InputPort == m_Lines[index].PortGuidB || edge.OutputPort == m_Lines[index].PortGuidB));
            edgesToRemove.AddRange(Owner.EditorView.DlogObject.GraphData.Edges.Where(edge => edge.InputPort == m_Lines[index].PortGuidC || edge.OutputPort == m_Lines[index].PortGuidC));
            Owner.EditorView.DlogObject.GraphData.RemoveElements(new List<SerializedNode>(), edgesToRemove);
            Owner.PortData.Remove(m_Lines[index].PortGuidA);
            Owner.PortData.Remove(m_Lines[index].PortGuidB);
            Owner.PortData.Remove(m_Lines[index].PortGuidC);
            Port portA = Owner.GuidPortDictionary[m_Lines[index].PortGuidA];
            Port portB = Owner.GuidPortDictionary[m_Lines[index].PortGuidB];
            Port portC = Owner.GuidPortDictionary[m_Lines[index].PortGuidC];
            Owner.GuidPortDictionary.Remove(m_Lines[index].PortGuidA);
            Owner.GuidPortDictionary.Remove(m_Lines[index].PortGuidB);
            Owner.GuidPortDictionary.Remove(m_Lines[index].PortGuidC);
            Ports.Remove(portA);
            Ports.Remove(portB);
            Ports.Remove(portC);
            m_Lines.RemoveAt(index);
            outputContainer.Remove(container);
            Refresh();
        }
    }
}