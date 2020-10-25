using System;
using System.Collections.Generic;

namespace Dlog.Runtime {
    [Serializable]
    public class ConversationLine {
        public string Message;
        public Guid Next;
        public Guid TriggerPort;
        public Guid CheckPort;
        public List<Guid> Triggers;
        public List<Guid> Checks;
    }
}