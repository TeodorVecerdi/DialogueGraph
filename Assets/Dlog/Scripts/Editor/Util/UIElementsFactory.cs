using System;
using UnityEngine.UIElements;

namespace Dlog {
    public static class UIElementsFactory {
        public static VisualElement FlexBreaker() {
            return VisualElement<VisualElement>("", new[] {"flex-break"});
        }

        public static Button Button(string buttonText, string name, string buttonTooltip, string[] classNames, Action onClick) {
            var button = TextElement<Button>(name, buttonText, classNames);
            if (!string.IsNullOrEmpty(buttonTooltip)) button.tooltip = buttonTooltip;
            if (onClick != null) button.clicked += onClick;
            return button;
        }

        public static T VisualElement<T>(string name, string[] classNames) where T : VisualElement, new() {
            var element = new T();
            if (!string.IsNullOrEmpty(name)) element.name = name;
            if(classNames != null) foreach (var className in classNames) element.AddToClassList(className);
            return element;
        }

        public static T TextElement<T>(string name, string text, string[] classNames) where T : TextElement, new() {
            var element = new T();
            if (!string.IsNullOrEmpty(name)) element.name = name;
            if (!string.IsNullOrEmpty(text)) element.text = text;
            if(classNames != null) foreach (var className in classNames) element.AddToClassList(className);
            return element;
        }

        public static TextField TextField(string name, string label, string[] classNames, EventCallback<ChangeEvent<string>> onChanged, EventCallback<FocusOutEvent> onFocusOut, bool multiLine = false, int maxLength = -1, bool isPasswordField = false, char passwordMaskChar = ' ') {
            var textField = VisualElement<TextField>(name, classNames);
            if (!string.IsNullOrEmpty(label)) textField.label = label;
            if (onChanged != null) textField.RegisterValueChangedCallback(onChanged);
            if (onFocusOut != null) textField.RegisterCallback(onFocusOut);
            textField.multiline = multiLine;
            textField.isPasswordField = isPasswordField;
            textField.maskChar = passwordMaskChar;
            textField.maxLength = maxLength;
            return textField;
        }
    }
}