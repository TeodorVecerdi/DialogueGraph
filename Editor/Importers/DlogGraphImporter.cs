using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DialogueGraph.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace DialogueGraph {
    [ScriptedImporter(0, EXTENSION, 3)]
    public class DlogGraphImporter : ScriptedImporter {
        public const string EXTENSION = "dlog";

        public override void OnImportAsset(AssetImportContext ctx) {
            try {
                string importedAssetPath = ctx.assetPath;

                DlogGraphObject graphObject = DialogueGraphUtility.LoadGraphAtPath(importedAssetPath);
                graphObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;

                if (string.IsNullOrEmpty(graphObject.AssetGuid) || graphObject.AssetGuid != AssetDatabase.AssetPathToGUID(importedAssetPath)) {
                    graphObject.RecalculateAssetGuid(importedAssetPath);
                    DialogueGraphUtility.SaveGraph(graphObject, false);
                }

                DlogObject runtimeObject = LoadRuntimeObject(importedAssetPath, graphObject.GraphData);
                Texture2D runtimeIcon = Resources.Load<Texture2D>(ResourcesUtility.RUNTIME_ICON_BIG);

                ctx.AddObjectToAsset("EditorGraph", graphObject);
                ctx.AddObjectToAsset("MainAsset", runtimeObject, runtimeIcon);
                ctx.SetMainObject(runtimeObject);
            } catch (Exception) {
                if (!DialogueGraphUtility.VersionMismatch(ctx.assetPath)) {
                    throw;
                }

                this.ImportInvalidVersion(ctx);
            }
        }

        private static DlogObject LoadRuntimeObject(string importedAssetPath, DlogGraphData graphData) {
            DlogObject runtimeObject = ScriptableObject.CreateInstance<DlogObject>();
            string assetName = Path.GetFileNameWithoutExtension(importedAssetPath);
            runtimeObject.name = assetName + " (Runtime)";

            // Add properties
            runtimeObject.Properties = graphData.Properties.Select(property => property.ToRuntime()).ToList();

            // Add nodes
            runtimeObject.Nodes = new List<Node>();
            foreach (SerializedNode node in graphData.Nodes) {
                JObject nodeData = JObject.Parse(node.NodeData);
                Node runtimeNode = new() {
                    Guid = node.GUID,
                    Type = node.Type switch {
                        "DialogueGraph.SelfNode" => NodeType.SELF,
                        "DialogueGraph.NpcNode" => NodeType.NPC,
                        "DialogueGraph.PropertyNode" => NodeType.PROP,
                        "DialogueGraph.NotBooleanNode" => NodeType.BOOLEAN_NOT,
                        "DialogueGraph.AndBooleanNode" => NodeType.BOOLEAN_AND,
                        "DialogueGraph.OrBooleanNode" => NodeType.BOOLEAN_OR,
                        "DialogueGraph.XorBooleanNode" => NodeType.BOOLEAN_XOR,
                        "DialogueGraph.NandBooleanNode" => NodeType.BOOLEAN_NAND,
                        "DialogueGraph.NorBooleanNode" => NodeType.BOOLEAN_NOR,
                        "DialogueGraph.XnorBooleanNode" => NodeType.BOOLEAN_XNOR,
                        _ => throw new NotSupportedException($"Invalid node type {node.Type}."),
                    },
                };

                if (runtimeNode.Type == NodeType.SELF) {
                    List<LineDataSelf> lines = JsonConvert.DeserializeObject<List<LineDataSelf>>(nodeData.Value<string>("lines"));
                    runtimeNode.Lines = lines.Select(line => line.ToRuntime()).ToList();
                } else if (runtimeNode.Type == NodeType.NPC) {
                    List<LineDataNpc> lines = JsonConvert.DeserializeObject<List<LineDataNpc>>(nodeData.Value<string>("lines"));
                    runtimeNode.Lines = lines.Select(line => line.ToRuntime()).ToList();
                } else if (runtimeNode.Type is NodeType.PROP) {
                    runtimeNode.Temp_PropertyNodeGuid = nodeData.Value<string>("propertyGuid");
                }

                runtimeObject.Nodes.Add(runtimeNode);
            }

            // Add edges
            runtimeObject.Edges = graphData.Edges.Select(edge => edge.ToRuntime()).ToList();
            runtimeObject.BuildGraph();
            return runtimeObject;
        }

        private void ImportInvalidVersion(AssetImportContext ctx) {
            Texture2D icon = Resources.Load<Texture2D>(ResourcesUtility.ICON_ERROR);
            VersionMismatchObject versionMismatchObject = ScriptableObject.CreateInstance<VersionMismatchObject>();
            ctx.AddObjectToAsset("MainAsset", versionMismatchObject, icon);
            ctx.SetMainObject(versionMismatchObject);
        }
    }
}