using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Dlog {
    internal class CreateDlogGraph : EndNameEditAction {
        [MenuItem("Assets/Create/Dialogue Graph/Empty Dialogue Graph", false, 1)]
        public static void CreateDialogueGraph()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateDlogGraph>(),
                $"New Dialogue Graph.{DlogGraphImporter.Extension}", Resources.Load<Texture2D>(ResourcesUtility.IconBig), null);
        }
        public override void Action(int instanceId, string pathName, string resourceFile) {
            var dlogGraph = new DlogGraphData();
            var dlogObject = CreateInstance<DlogGraphObject>();
            dlogObject.Initialize(dlogGraph);
            dlogObject.DlogGraph.AssetGuid = AssetDatabase.GetAssetPath(instanceId);
            dlogObject.AssetGuid = dlogObject.DlogGraph.AssetGuid;
            DlogUtility.CreateFile(pathName, dlogObject, false);
            AssetDatabase.ImportAsset(pathName);
        }
    }
}