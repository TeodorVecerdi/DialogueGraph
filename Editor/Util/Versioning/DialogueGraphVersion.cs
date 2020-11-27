using System;
using UnityEditor;
using UnityEngine;

namespace Dlog {
    [CreateAssetMenu(fileName = "Versioning", menuName = "Dialogue Graph/Versioning", order = 0)]
    public class DialogueGraphVersion : ScriptableObject {
        [SerializeField] private SemVer version;
        public SemVer Version {
            get => version;
            set => version = value;
        }

        public void Apply() {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }

    [CustomEditor(typeof(DialogueGraphVersion))]
    public class DialogueGraphVersionEditor : Editor {
        private GUIStyle bigLabel = null;
        private GUIStyle button = null;

        private string versionString;
        private bool isValid;
        private bool changed;
        private DialogueGraphVersion version;
        private bool shouldSetStyles;

        private void OnEnable() {
            version = target as DialogueGraphVersion;
            versionString = version.Version;
            isValid = SemVer.IsValid(versionString);
            changed = false;
            shouldSetStyles = true;
        }

        public override void OnInspectorGUI() {
            if (shouldSetStyles) {
                SetStyles();
                shouldSetStyles = false;
            }
            
            GUILayout.Label($"Current version {version.Version}", bigLabel);

            EditorGUI.BeginChangeCheck();
            versionString = EditorGUILayout.TextField("Version (SemVer)", versionString);
            if (EditorGUI.EndChangeCheck()) {
                isValid = SemVer.IsValid(versionString);
                changed = versionString != version.Version.ToString();
            }

            GUI.enabled = isValid && changed;
            if (GUILayout.Button("Apply", button)) {
                version.Version = (SemVer) versionString;
                version.Apply();
                changed = false;
            }

            GUI.enabled = true;
        }
        
        private void SetStyles() {
            bigLabel = new GUIStyle(GUI.skin.label);
            button = new GUIStyle(GUI.skin.button) {alignment = TextAnchor.MiddleCenter};

            bigLabel.fontSize = 28;
            bigLabel.fontStyle = FontStyle.Bold;
            bigLabel.richText = true;
            button.fontSize = 14;
            button.richText = true;
        }
    }
}