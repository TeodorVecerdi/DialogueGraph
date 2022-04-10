using System;
using System.Collections.Generic;

namespace DialogueGraph.Runtime {
    [Serializable]
    public class ConversationLine {
        public string Message;
        public string Next;
        public string TriggerPort;
        public string CheckPort;
        public List<string> Triggers;
        public List<string> Checks;
    }
}