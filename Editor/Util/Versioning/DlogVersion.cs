using UnityEditor;
using UnityEngine;

namespace Dlog {
    public static class DlogVersion {
        private static readonly SemVer FallbackVersion = (SemVer)"1.1.1";
        private static SemVer lastCommittedVersion = SemVer.Invalid;
        private static Ref<SemVer> version;

        public static SemVer CommittedVersion {
            get {
                if (lastCommittedVersion == SemVer.Invalid)
                    lastCommittedVersion = Load();
                return lastCommittedVersion;
            }
            private set {
                lastCommittedVersion = value;
                Save();
            }
        }

        public static Ref<SemVer> Version {
            get {
                if (version == null) {
                    version = Load();
                    lastCommittedVersion = version.GetVal();
                }

                return version;
            }
            set => version = value;
        }

        private static void Save() {
            EditorPrefs.SetString("DialogueGraphVersion", version.Get());
        }

        private static SemVer Load() {
            if (!EditorPrefs.HasKey("DialogueGraphVersion"))
                EditorPrefs.SetString("DialogueGraphVersion", FallbackVersion);

            return (SemVer)EditorPrefs.GetString("DialogueGraphVersion");
        }

        public static void ResetToCommittedVersion() {
            Version = CommittedVersion;
        }

        public static void CommitVersion() {
            CommittedVersion = Version.Get();
        }

        [MenuItem("Dialogue Graph/Versioning")]
        private static void OpenMenu() {
            var window = EditorWindow.GetWindow<DlogVersioningWindow>();
            window.titleContent = new GUIContent("Dialogue Graph - Versioning");
            var minSize = window.minSize;
            minSize.x = 500;
            window.minSize = minSize;
            window.Show();
        }
    }
}