using System;
using UnityEditor;
using UnityEngine;

namespace Dlog {
    public class DlogVersioningWindow : EditorWindow {
        private GUIStyle bigLabel = null;
        private GUIStyle bigLabelCenter = null;
        private GUIStyle bigLabelRight = null;
        private GUIStyle label = null;
        private GUIStyle labelCenter = null;
        private GUIStyle labelRight = null;
        private GUIStyle button = null;
        private GUIStyle smallButton = null;

        private string enteredVersion = "0.0.0";

        private bool shouldSetStyles;

        private void OnEnable() {
            shouldSetStyles = true;
        }

        private void SetStyles() {
            bigLabel = new GUIStyle(GUI.skin.label);
            label = new GUIStyle(GUI.skin.label);
            button = new GUIStyle(GUI.skin.button) {alignment = TextAnchor.MiddleCenter};

            bigLabel.fontSize = 28;
            bigLabel.fontStyle = FontStyle.Bold;
            bigLabel.richText = true;
            bigLabelCenter = new GUIStyle(bigLabel) {alignment = TextAnchor.MiddleCenter};
            bigLabelRight = new GUIStyle(bigLabel) {alignment = TextAnchor.MiddleRight};
            label.fontSize = 14;
            label.richText = true;
            labelCenter = new GUIStyle(label) {alignment = TextAnchor.MiddleCenter};
            labelRight = new GUIStyle(label) {alignment = TextAnchor.MiddleRight};
            button.fontSize = 14;
            button.richText = true;
            smallButton = new GUIStyle(button) {fontSize = 10};
        }

        private void OnGUI() {
            if (shouldSetStyles) {
                SetStyles();
                shouldSetStyles = false;
            }

            var guiColor = GUI.color;
            GUILayout.Label("Versioning", bigLabelCenter);
            GUILayout.BeginHorizontal();
            GUI.color = Color.yellow;
            GUILayout.Label($"Current {DlogVersion.Version.Get()}", labelRight);
            GUILayout.Label($"(Committed {DlogVersion.CommittedVersion})", label);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset", smallButton)) {
                DlogVersion.ResetToCommittedVersion();
            }
            if (GUILayout.Button("Commit", smallButton)) {
                DlogVersion.CommitVersion();
            }
            GUILayout.FlexibleSpace();
            GUI.color = guiColor;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.MaxHeight(32));
            GUI.backgroundColor = Color.red;
            GUILayout.Space(32);
            GUILayoutHelper.CenterVertically(() => {
                if (GUILayout.Button("Bump MAJOR")) {
                    DlogVersion.Version.Get().BumpMajor();
                }
            });
            GUILayoutHelper.CenterVertically(() => {
                if (GUILayout.Button("Bump MINOR")) {
                    DlogVersion.Version.Get().BumpMinor();
                }
            });
            GUILayoutHelper.CenterVertically(() => {
                if (GUILayout.Button("Bump PATCH")) {
                    DlogVersion.Version.Get().BumpPatch();
                }
            });
            GUILayout.Space(32);
            GUI.backgroundColor = guiColor;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.MaxHeight(32));
            GUILayout.Space(32);
            GUILayoutHelper.CenterVertically(() => {
                GUILayout.Label("(Enter Manually) Version", labelRight);
            });
            GUILayoutHelper.CenterVertically(() => {
                enteredVersion = GUILayout.TextField(enteredVersion);
            });
            if (!SemVer.IsValid(enteredVersion)) GUI.enabled = false;
            GUILayoutHelper.CenterVertically(() => {
                if (GUILayout.Button("Set", button))
                    DlogVersion.Version = (SemVer) enteredVersion;
            });
            GUI.enabled = true;
            GUILayout.Space(32);
            GUILayout.EndHorizontal();
        }
    }
}