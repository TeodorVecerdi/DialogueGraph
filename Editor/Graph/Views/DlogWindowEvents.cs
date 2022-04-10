using System;

namespace DialogueGraph {
    public class DlogWindowEvents {
        public Action SaveRequested;
        public Func<bool> SaveAsRequested;
        public Action ShowInProjectRequested;
    }
}