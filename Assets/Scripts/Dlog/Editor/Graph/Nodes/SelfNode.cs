using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public class LineData {
        public string Line;
        public string PortGuidA;
        public string PortGuidB;
    }

    [Title("Self")]
    public class SelfNode : AbstractNode {
        public List<LineData> Lines = new List<LineData>();

        private bool first = true;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Self", EditorView.DefaultNodePosition);

            var button = new Button(() => AddConversationPort(true)) {text = "Create Dialogue Line"};
            extensionContainer.Add(button);

            var titleLabel = new Label {name = "lineTitle", text = "Lines"};
            outputContainer.Add(titleLabel);

            AddPort(DlogPort.Create("Branch", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object), edgeConnectorListener));
            Refresh();
        }

        public override void SetNodeData(string jsonData) {
            if (string.IsNullOrEmpty(jsonData)) return;
            base.SetNodeData(jsonData);
            var data = JObject.Parse(jsonData);
            var lines = JsonConvert.DeserializeObject<List<LineData>>(data.Value<string>("lines"));
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
            Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Created dialogue line");
            var conversationContainer = new VisualElement {name = "conversation-container"};
            var message = new TextField("Line", -1, true, false, ' ') {name = "conversation-item"};
            if (create) {
                index = Lines.Count;
                Lines.Add(new LineData {Line = ""});
            }

            message.RegisterValueChangedCallback(evt => {
                Owner.EditorView.DlogObject.RegisterCompleteObjectUndo("Changed dialogue line");
                Lines[index].Line = evt.newValue;
            });
            message.AddToClassList("message");
            if (!create) {
                message.SetValueWithoutNotify(Lines[index].Line);
            }

            var branchPort = DlogPort.Create("Branch", Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(object), EdgeConnectorListener);
            branchPort.name = "conversation-item";
            branchPort.AddToClassList("branch-port");
            var triggerPort = DlogPort.Create("Trigger", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object), EdgeConnectorListener);
            triggerPort.name = "conversation-item";
            triggerPort.AddToClassList("trigger-port");

            if (create) {
                Lines[index].PortGuidA = branchPort.viewDataKey;
                Lines[index].PortGuidB = triggerPort.viewDataKey;
            } else {
                branchPort.viewDataKey = Lines[index].PortGuidA;
                triggerPort.viewDataKey = Lines[index].PortGuidB;
            }

            conversationContainer.Add(message);
            conversationContainer.Add(branchPort);
            conversationContainer.Add(triggerPort);

            if (!first) {
                var separator = new VisualElement {name = "divider"};
                separator.AddToClassList("horizontal");
                separator.AddToClassList("horizontal-divider");
                outputContainer.Add(separator);
            } else {
                first = false;
            }

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
    }
}