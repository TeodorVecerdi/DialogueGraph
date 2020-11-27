using UnityEditor;
using UnityEngine;

namespace Dlog {
    public static class DlogVersion {
        private static readonly SemVer FallbackVersion = (SemVer) "1.1.1";
        private static SemVer lastCommittedVersion = SemVer.Invalid;
        private static Ref<SemVer> version;
        private static DialogueGraphVersion versionFile;

        public static SemVer CommittedVersion {
            get {
                if (lastCommittedVersion == SemVer.Invalid) {
                    LoadVersionFile();
                }

                lastCommittedVersion = versionFile.Version;
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
                    LoadVersionFile();
                    version = versionFile.Version;
                    lastCommittedVersion = version.GetValue();
                }

                return version;
            }
            set => version = value;
        }

        private static void Save() {
            versionFile.Version = version.GetValue();
        }

        public static void ResetToCommittedVersion() {
            Version = CommittedVersion;
        }

        public static void CommitVersion() {
            CommittedVersion = Version.Get();
        }

        private static void LoadVersionFile() {
            if (versionFile == null) {
                versionFile = Resources.Load<DialogueGraphVersion>("DialogueGraphVersion");
            }

            if (versionFile == null) {
                Debug.LogWarning($"Unable to load DialogueGraphVersion. Creating a new file with fallback version = {FallbackVersion}");
                versionFile = ScriptableObject.CreateInstance<DialogueGraphVersion>();
                versionFile.Version = FallbackVersion;
#if DLOG_DEV
                AssetDatabase.CreateAsset(versionFile, "Assets/DialogueGraph/Resources/DialogueGraphVersion.asset");
#endif
            }
        }
    }
}