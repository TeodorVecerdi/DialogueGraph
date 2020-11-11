using System;
using System.Collections.Generic;
using System.Linq;
using Dlog.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Dlog {
    [ScriptedImporter(0, Extension, 3)]
    public class DlogGraphImporter : ScriptedImporter {
        public const string Extension = "dlog";
        
        public override void OnImportAsset(AssetImportContext ctx) {
            var dlogObject = DlogUtility.LoadGraphAtPath(ctx.assetPath);
            var icon = Resources.Load<Texture2D>(ResourcesUtility.IconBig);
            var runtimeIcon = Resources.Load<Texture2D>(ResourcesUtility.RuntimeIconBig);
            
            if (string.IsNullOrEmpty(dlogObject.AssetGuid) || dlogObject.AssetGuid != AssetDatabase.AssetPathToGUID(ctx.assetPath)) {
                dlogObject.RecalculateAssetGuid(ctx.assetPath);
                DlogUtility.SaveGraph(dlogObject, false);
            }
            
            ctx.AddObjectToAsset("MainAsset", dlogObject, icon);
            ctx.SetMainObject(dlogObject);

            var runtimeObject = ScriptableObject.CreateInstance<DlogObject>();
            var filePath = ctx.assetPath;
            var assetNameSubStartIndex = filePath.LastIndexOf('/') + 1;
            var assetNameSubEndIndex = filePath.LastIndexOf('.');
            var assetName = filePath.Substring(assetNameSubStartIndex, assetNameSubEndIndex-assetNameSubStartIndex);
            runtimeObject.name = assetName + " (Runtime)";
            // Add properties
            runtimeObject.Properties = new List<Runtime.Property>();
            runtimeObject.Properties.AddRange(dlogObject.DlogGraph.Properties.Select(
                property =>
                    new Runtime.Property {
                        Type = property.Type, DisplayName = property.DisplayName, ReferenceName = property.ReferenceName, Guid = property.GUID
                    }
            ));

            // Add nodes
            runtimeObject.Nodes = new List<Runtime.Node>();
            foreach (var node in dlogObject.DlogGraph.Nodes) {
                var nodeData = JObject.Parse(node.NodeData);

                var runtimeNode = new Runtime.Node();
                runtimeNode.Guid = node.GUID;
                switch (node.Type) {
                    case "Dlog.SelfNode":
                        runtimeNode.Type = NodeType.SELF;
                        break;
                    case "Dlog.NpcNode":
                        runtimeNode.Type = NodeType.NPC;
                        break;
                    case "Dlog.PropertyNode":
                        runtimeNode.Type = NodeType.PROP;
                        runtimeNode.Temp_PropertyNodeGuid = nodeData.Value<string>("propertyGuid");
                        break;
                    default:
                        throw new NotSupportedException($"Invalid node type {node.Type}.");
                }

                // Get lines
                if (runtimeNode.Type != NodeType.PROP) {
                    runtimeNode.Lines = new List<ConversationLine>();
                    if (runtimeNode.Type == NodeType.SELF) {
                        var lines = JsonConvert.DeserializeObject<List<LineDataSelf>>(nodeData.Value<string>("lines"));
                        foreach (var line in lines) {
                            var runtimeLine = new ConversationLine {Message = line.Line, Next = line.PortGuidA, TriggerPort = line.PortGuidB, CheckPort = Guid.Empty.ToString()};
                            runtimeNode.Lines.Add(runtimeLine);
                        }
                    } else {
                        var lines = JsonConvert.DeserializeObject<List<LineDataNpc>>(nodeData.Value<string>("lines"));
                        foreach (var line in lines) {
                            var runtimeLine = new ConversationLine {Message = line.Line, Next = line.PortGuidA, TriggerPort = line.PortGuidB, CheckPort = line.PortGuidC};
                            runtimeNode.Lines.Add(runtimeLine);
                        }
                    }
                }

                runtimeObject.Nodes.Add(runtimeNode);
            }

            // Add edges
            runtimeObject.Edges = new List<Runtime.Edge>();
            runtimeObject.Edges.AddRange(dlogObject.DlogGraph.Edges.Select(
                edge =>
                    new Runtime.Edge {
                        FromNode = edge.Output, FromPort = edge.OutputPort, ToNode = edge.Input, ToPort = edge.InputPort
                    }
            ));
            runtimeObject.BuildGraph();
            
            ctx.AddObjectToAsset("RuntimeAsset", runtimeObject, runtimeIcon);
            AssetDatabase.Refresh();
            
            EditorUtility.SetDirty(runtimeObject);
        }
    }
}