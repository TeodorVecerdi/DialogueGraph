using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
#pragma warning disable 618

namespace Dlog {
    public class LineDataSelf {
        public string Line;
        public string PortGuidA;
        public string PortGuidB;
    }

    [Title("Self")]
    public class SelfNode : AbstractNode {
        public List<LineDataSelf> Lines = new List<LineDataSelf>();

        private VisualElement lineLabel;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Self", EditorView.DefaultNodePosition);

            lineLabel = new Label {name = "lineTitle", text = "Lines"};
            outputContainer.Add(lineLabel);

            var button = new Button(() => AddConversationPort(true)) {text = "Create Dialogue Line"};
            extensionContainer.Add(button);
            var titleLabel = this.Q<Label>("title-label");
            var titleElement = this.Q("title");
            var titleC = UIElementsFactory.VisualElement<VisualElement>("title-container", null);
            titleLabel.RemoveFromHierarchy();
            titleC.Add(titleLabel);
            titleElement.Insert(0, titleC);

            var branchPort = DlogPort.Create("Branch", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, PortType.Branch, edgeConnectorListener);
            titleC.Insert(0, branchPort);
            AddPort(branchPort, false);
            Refresh();
        }

        public override void SetNodeData(string jsonData) {
            if (string.IsNullOrEmpty(jsonData)) return;
            base.SetNodeData(jsonData);
            var data = JObject.Parse(jsonData);
            var lines = JsonConvert.DeserializeObject<List<LineDataSelf>>(data.Value<string>("lines"));
            Lines.Clear();
            Lines.AddRange(lines);
            for (int i = 0; i < Lines.Count; i++) {
                AddConversationPort(false, i);
            }
        }

        public override string GetNodeData() {
            var root = new JObject();
            root["lines"] = new JValue(JsonConvert.SerializeObject(Lines));
            root.Merge(JObject.Parse(base.GetNodeData()));
            return root.ToString(Formatting.None);
        }

        private void AddConversationPort(bool create, int index = -1) {
            var conversationContainer = new VisualElement {name = "conversation-container"};
            lineLabel.AddToClassList("visible");

            if (create) {
                Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Created Dialogue Line");
                index = Lines.Count;
                Lines.Add(new LineDataSelf {Line = ""});
            }

            var message = UIElementsFactory.TextField("conversation-item", "Line", new[] {"message"}, null, null, true);
            if (!create) {
                message.SetValueWithoutNotify(Lines[index].Line);
            }

            var branchPort = DlogPort.Create("Branch", Orientation.Horizontal, Direction.Output, Port.Capacity.Single, PortType.Branch, EdgeConnectorListener);
            branchPort.name = "conversation-item";
            branchPort.AddToClassList("branch-port");
            var triggerPort = DlogPort.Create("Trigger", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Trigger, EdgeConnectorListener);
            triggerPort.name = "conversation-item";
            triggerPort.AddToClassList("trigger-port");

            var flexBreak = UIElementsFactory.FlexBreaker();
            if (create) {
                Lines[index].PortGuidA = branchPort.viewDataKey;
                Lines[index].PortGuidB = triggerPort.viewDataKey;
            } else {
                branchPort.viewDataKey = Lines[index].PortGuidA;
                triggerPort.viewDataKey = Lines[index].PortGuidB;
            }

            message.RegisterCallback<FocusOutEvent>(evt => {
                var lineIndex = Lines.FindIndex(data => data.PortGuidA == branchPort.viewDataKey);
                if (message.value != Lines[lineIndex].Line) {
                    Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Changed Dialogue Line");
                    Lines[lineIndex].Line = message.value;
                }
            });
            var removeButton = UIElementsFactory.Button("x", "conversation-item", "Remove line", new[] {"remove-button"}, () => { RemoveLine(Lines.FindIndex(data => data.PortGuidA == branchPort.viewDataKey)); });

            conversationContainer.Add(message);
            conversationContainer.Add(branchPort);
            conversationContainer.Add(flexBreak);
            conversationContainer.Add(removeButton);
            conversationContainer.Add(triggerPort);

            var separator = new VisualElement {name = "divider"};
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
            if (Lines.Count == 1)
                lineLabel.RemoveFromClassList("visible");
            var container = outputContainer.Children().Where(element => element.name == "conversation-container").ToList()[index];
            var separator = outputContainer.Children().Where(element => element.name == "divider" && element.ClassListContains("horizontal-divider")).ToList()[index];
            outputContainer.Remove(separator);

            Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Removed Line");
            var edgesToRemove = Owner.EditorView.DlogObject.DlogGraph.Edges.Where(edge => edge.InputPort == Lines[index].PortGuidA || edge.OutputPort == Lines[index].PortGuidA).ToList();
            edgesToRemove.AddRange(Owner.EditorView.DlogObject.DlogGraph.Edges.Where(edge => edge.InputPort == Lines[index].PortGuidB || edge.OutputPort == Lines[index].PortGuidB));
            Owner.EditorView.DlogObject.DlogGraph.RemoveElements(new List<SerializedNode>(), edgesToRemove);
            Owner.PortData.Remove(Lines[index].PortGuidA);
            Owner.PortData.Remove(Lines[index].PortGuidB);
            var portA = Owner.GuidPortDictionary[Lines[index].PortGuidA];
            var portB = Owner.GuidPortDictionary[Lines[index].PortGuidB];
            Owner.GuidPortDictionary.Remove(Lines[index].PortGuidA);
            Owner.GuidPortDictionary.Remove(Lines[index].PortGuidB);
            Ports.Remove(portA);
            Ports.Remove(portB);
            Lines.RemoveAt(index);
            outputContainer.Remove(container);
            Refresh();
        }
    }
}