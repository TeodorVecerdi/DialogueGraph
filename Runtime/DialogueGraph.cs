using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dlog.Runtime {
    public class DialogueGraph : MonoBehaviour {
        public DlogObject DlogObject;

        #region Inspector Data
        public string CurrentAssetGuid;
        public List<DlogObjectData> PersistentData = new List<DlogObjectData>();
        public StringIntSerializableDictionary PersistentDataIndices = new StringIntSerializableDictionary();
        public DlogObjectData CurrentData {
            get {
                if (string.IsNullOrEmpty(CurrentAssetGuid)) return null;
                if (!PersistentDataIndices.ContainsKey(CurrentAssetGuid)) {
                    PersistentDataIndices[CurrentAssetGuid] = PersistentData.Count;
                    PersistentData.Add(new DlogObjectData());
                }

                return PersistentData[PersistentDataIndices[CurrentAssetGuid]];
            }
        }
        public int CurrentIndex {
            get {
                if (string.IsNullOrEmpty(CurrentAssetGuid)) return -1;
                if (!PersistentDataIndices.ContainsKey(CurrentAssetGuid)) {
                    PersistentDataIndices[CurrentAssetGuid] = PersistentData.Count;
                    PersistentData.Add(new DlogObjectData());
                }

                return PersistentDataIndices[CurrentAssetGuid];
            }
        }
        public void ClearData() {
            PersistentDataIndices = new StringIntSerializableDictionary();
            PersistentData = new List<DlogObjectData>();
        }
        #endregion

        private bool conversationDone;
        private string currentNodeGuid;

        public void ResetConversation() {
            conversationDone = false;
            currentNodeGuid = DlogObject.StartNode;
        }

        public void EndConversation() {
            conversationDone = true;
            currentNodeGuid = null;
        }

        public bool IsCurrentNpc() {
            var currentNode = DlogObject.NodeDictionary[currentNodeGuid];
            return currentNode.Type == NodeType.NPC;
        }

        public bool IsConversationDone() {
            return conversationDone;
        }

        public ActorData GetCurrentActor() {
            var currentNode = DlogObject.NodeDictionary[currentNodeGuid];
            if (currentNode.Type != NodeType.NPC) return null;
            var currentNodeActorGuid = currentNode.ActorGuid;
            var actor = CurrentData.ActorData[CurrentData.ActorDataIndices[currentNodeActorGuid]];
            return actor;
        }

        public List<ConversationLine> GetCurrentLines() {
            var currentNode = DlogObject.NodeDictionary[currentNodeGuid];
            return currentNode.Lines;
        }

        public string ProgressNpc() {
            var lines = GetCurrentLines();
            for (var i = 0; i < lines.Count - 1; i++) {
                var line = lines[i];
                var currentCheck = ExecuteChecks(line, i);
                

                if (currentCheck) {
                    Progress(line);
                    ExecuteTriggers(line, i);
                    return line.Message;
                }
            }

            var lastLine = lines[lines.Count - 1];
            Progress(lastLine);
            ExecuteTriggers(lastLine, lines.Count-1);
            return lastLine.Message;
        }

        public string ProgressSelf(int lineIndex) {
            var lines = GetCurrentLines();
            Progress(lines[lineIndex]);
            ExecuteTriggers(lines[lineIndex], lineIndex);
            return lines[lineIndex].Message;
        }

        private bool ExecuteChecks(ConversationLine line, int lineIndex) {
            var currentCheck = true;
            foreach (var checkGuid in line.Checks) {
                currentCheck &= CurrentData.CheckData[CurrentData.CheckDataIndices[checkGuid]].Invoke(currentNodeGuid, lineIndex);
            }

            return currentCheck;
        }

        private void ExecuteTriggers(ConversationLine line, int lineIndex) {
            foreach (var triggerGuid in line.Triggers) {
               CurrentData.TriggerData[CurrentData.TriggerDataIndices[triggerGuid]].Invoke(currentNodeGuid, lineIndex);
            }
        }

        private void Progress(ConversationLine line) {
            if (string.IsNullOrEmpty(line.Next)) {
                conversationDone = true;
                currentNodeGuid = null;
                return;
            }

            currentNodeGuid = line.Next;
        }
    }
}