using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Dlog {
    [ScriptedImporter(0, Extension, 3)]
    public class DlogGraphImporter : ScriptedImporter {
        public const string Extension = "dlog";
        public override void OnImportAsset(AssetImportContext ctx) {
            var dlogObject = SaveUtility.LoadAtPath(ctx.assetPath);
            var fileIcon = Resources.Load<Texture2D>(ResourcesUtility.IconBig);
            if (string.IsNullOrEmpty(dlogObject.AssetGuid) || dlogObject.AssetGuid != AssetDatabase.AssetPathToGUID(ctx.assetPath)) {
                dlogObject.RecalculateAssetGuid(ctx.assetPath);
            }
            ctx.AddObjectToAsset("MainAsset", dlogObject, fileIcon);
            ctx.SetMainObject(dlogObject);
        }
    }
}