using System;
using System.Collections.Generic;
using System.Linq;
using DialogueGraph.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace DialogueGraph {
    [ScriptedImporter(0, Extension, 3)]
    public class DlogGraphImporter : ScriptedImporter {
        public const string Extension = "dlog";

        public override void OnImportAsset(AssetImportContext ctx) {
            try {
                var dlogObject = DialogueGraphUtility.LoadGraphAtPath(ctx.assetPath);
                var icon = Resources.Load<Texture2D>(ResourcesUtility.IconBig);
                var runtimeIcon = Resources.Load<Texture2D>(ResourcesUtility.RuntimeIconBig);

                if (string.IsNullOrEmpty(dlogObject.AssetGuid) || dlogObject.AssetGuid != AssetDatabase.AssetPathToGUID(ctx.assetPath)) {
                    dlogObject.RecalculateAssetGuid(ctx.assetPath);
                    DialogueGraphUtility.SaveGraph(dlogObject, false);
                }

                ctx.AddObjectToAsset("EditorGraph", dlogObject, icon);
                dlogObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;

                var runtimeObject = ScriptableObject.CreateInstance<DlogObject>();
                var filePath = ctx.assetPath;
                var assetNameSubStartIndex = filePath.LastIndexOf('/') + 1;
                var assetNameSubEndIndex = filePath.LastIndexOf('.');
                var assetName = filePath.Substring(assetNameSubStartIndex, assetNameSubEndIndex - assetNameSubStartIndex);
                runtimeObject.name = assetName + " (Runtime)";

                // Add properties
                runtimeObject.Properties = new List<Property>();
                runtimeObject.Properties.AddRange(dlogObject.DlogGraph.Properties.Select(
                                                      property =>
                                                          new Property {
                                                              Type = property.Type, DisplayName = property.DisplayName, ReferenceName = property.ReferenceName, Guid = property.GUID
                                                          }
                                                  ));

                // Add nodes
                runtimeObject.Nodes = new List<Node>();
                foreach (var node in dlogObject.DlogGraph.Nodes) {
                    var nodeData = JObject.Parse(node.NodeData);

                    var runtimeNode = new Node();
                    runtimeNode.Guid = node.GUID;
                    switch (node.Type) {
                        case "DialogueGraph.SelfNode":
                            runtimeNode.Type = NodeType.SELF;
                            break;
                        case "DialogueGraph.NpcNode":
                            runtimeNode.Type = NodeType.NPC;
                            break;
                        case "DialogueGraph.PropertyNode":
                            runtimeNode.Type = NodeType.PROP;
                            runtimeNode.Temp_PropertyNodeGuid = nodeData.Value<string>("propertyGuid");
                            break;
                        case "DialogueGraph.NotBooleanNode":
                            runtimeNode.Type = NodeType.BOOLEAN_NOT;
                            break;
                        case "DialogueGraph.AndBooleanNode":
                            runtimeNode.Type = NodeType.BOOLEAN_AND;
                            break;
                        case "DialogueGraph.OrBooleanNode":
                            runtimeNode.Type = NodeType.BOOLEAN_OR;
                            break;
                        case "DialogueGraph.XorBooleanNode":
                            runtimeNode.Type = NodeType.BOOLEAN_XOR;
                            break;
                        case "DialogueGraph.NandBooleanNode":
                            runtimeNode.Type = NodeType.BOOLEAN_NAND;
                            break;
                        case "DialogueGraph.NorBooleanNode":
                            runtimeNode.Type = NodeType.BOOLEAN_NOR;
                            break;
                        case "DialogueGraph.XnorBooleanNode":
                            runtimeNode.Type = NodeType.BOOLEAN_XNOR;
                            break;
                        default:
                            throw new NotSupportedException($"Invalid node type {node.Type}.");
                    }

                    // Get lines
                    if (runtimeNode.Type == NodeType.SELF) {
                        runtimeNode.Lines = new List<ConversationLine>();
                        var lines = JsonConvert.DeserializeObject<List<LineDataSelf>>(nodeData.Value<string>("lines"));
                        foreach (var line in lines) {
                            var runtimeLine = new ConversationLine {Message = line.Line, Next = line.PortGuidA, TriggerPort = line.PortGuidB, CheckPort = Guid.Empty.ToString()};
                            runtimeNode.Lines.Add(runtimeLine);
                        }
                    } else if (runtimeNode.Type == NodeType.NPC) {
                        runtimeNode.Lines = new List<ConversationLine>();
                        var lines = JsonConvert.DeserializeObject<List<LineDataNpc>>(nodeData.Value<string>("lines"));
                        foreach (var line in lines) {
                            var runtimeLine = new ConversationLine {Message = line.Line, Next = line.PortGuidA, TriggerPort = line.PortGuidB, CheckPort = line.PortGuidC};
                            runtimeNode.Lines.Add(runtimeLine);
                        }
                    }

                    runtimeObject.Nodes.Add(runtimeNode);
                }

                // Add edges
                runtimeObject.Edges = new List<Edge>();
                runtimeObject.Edges.AddRange(dlogObject.DlogGraph.Edges.Select(
                                                 edge =>
                                                     new Edge {
                                                         FromNode = edge.Output, FromPort = edge.OutputPort, ToNode = edge.Input, ToPort = edge.InputPort
                                                     }
                                             ));
                runtimeObject.BuildGraph();

                ctx.AddObjectToAsset("MainAsset", runtimeObject, runtimeIcon);
                ctx.SetMainObject(runtimeObject);
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(runtimeObject);
            } catch (Exception) {
                if (DialogueGraphUtility.VersionMismatch(ctx.assetPath)) {
                    ImportInvalidVersion(ctx);
                    return;
                }

                throw;
            }
        }

        private void ImportInvalidVersion(AssetImportContext ctx) {
            var icon = Resources.Load<Texture2D>(ResourcesUtility.IconError);
            VersionMismatchObject versionMismatchObject = ScriptableObject.CreateInstance<VersionMismatchObject>();
            ctx.AddObjectToAsset("MainAsset", versionMismatchObject, icon);
            ctx.SetMainObject(versionMismatchObject);
        }
    }
}