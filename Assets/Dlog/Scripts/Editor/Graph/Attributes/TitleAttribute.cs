using System;

namespace Dlog {
    [AttributeUsage(AttributeTargets.Class)]
    public class TitleAttribute : Attribute {
        public readonly string[] Title;
        public TitleAttribute(params string[] title) {
            Title = title;
        }
    }
}