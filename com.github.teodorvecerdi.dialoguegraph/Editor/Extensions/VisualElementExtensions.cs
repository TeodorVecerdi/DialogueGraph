using UnityEngine;
using UnityEngine.UIElements;

namespace Dlog {
    public static class VisualElementExtensions {
        public static void AddStyleSheet(this VisualElement element, string path) {
            var stylesheet = Resources.Load<StyleSheet>(path);
            if(stylesheet == null) Debug.LogWarning($"StyleSheet at path \"{path}\" could not be found");
            else element.styleSheets.Add(stylesheet);
        }
    }
}