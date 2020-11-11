using System;

namespace Dlog {
    public class DlogWindowEvents {
        public Action SaveRequested;
        public Func<bool> SaveAsRequested;
        public Action ShowInProjectRequested;
    }
}