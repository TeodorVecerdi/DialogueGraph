using System;

namespace DialogueGraph {
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false), ]
    public class ConvertMethodAttribute : Attribute {
        public readonly SemVer TargetVersion;

        /// <summary>
        /// Specifies that tha attached method is a converting method (from one version of Dialogue Graph to another)
        /// </summary>
        /// <param name="targetVersion">The target version the method converts to.</param>
        public ConvertMethodAttribute(string targetVersion) {
            TargetVersion = (SemVer)targetVersion;
        }
    }
}