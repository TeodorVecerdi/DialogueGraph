using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Dlog {
    public static class DlogVersion {
        private static readonly SemVer FallbackVersion = (SemVer) "1.1.1";
        
        private static Ref<SemVer> version;
        private static DialogueGraphVersion versionFile;

        public static Ref<SemVer> Version {
            get {
                if (version == null) {
                    LoadVersionFile();
                    version = Ref<SemVer>.MakeRef(versionFile.Version, () => versionFile.Version, () => versionFile.Version = version.GetValueUnbound());
                }

                return version;
            }
        }

        public static void SaveVersion(SemVer newVersion) {
            Version.Set(newVersion);
            versionFile.Apply();
            
            // Load package.json file and update version
            var packagePath = $"{DlogUtility.DialogueGraphPath}\\package.json";
            var packageText = File.ReadAllText(packagePath);
            var package = JObject.Parse(packageText);
            package["version"] = versionFile.Version.ToString();
            File.WriteAllText(packagePath, package.ToString(Formatting.Indented));
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