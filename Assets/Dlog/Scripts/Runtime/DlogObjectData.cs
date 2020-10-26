using System;
using System.Collections.Generic;
using Dlog.Runtime;

namespace Dlog.Scripts.Runtime {
    [Serializable]
    public class DlogObjectData {
        public StringIntSerializableDictionary ActorDataIndices;
        public List<ActorData> ActorData;
        public StringIntSerializableDictionary CheckDataIndices;
        public List<CheckEvent> CheckData;
        public StringIntSerializableDictionary TriggerDataIndices;
        public List<TriggerEvent> TriggerData;

        public DlogObjectData() {
            ActorDataIndices = new StringIntSerializableDictionary();
            CheckDataIndices = new StringIntSerializableDictionary();
            TriggerDataIndices = new StringIntSerializableDictionary();
            ActorData = new List<ActorData>();
            CheckData = new List<CheckEvent>();
            TriggerData = new List<TriggerEvent>();
        }

        public void AddActorData(string guid, ActorData data) {
            ActorDataIndices[guid] = ActorData.Count;
            ActorData.Add(data);
        }
        
        public void AddCheckEvent(string guid, CheckEvent evt) {
            CheckDataIndices[guid] = CheckData.Count;
            CheckData.Add(evt);
            evt.dynamic = true;
        }
        
        public void AddTriggerEvent(string guid, TriggerEvent evt) {
            TriggerDataIndices[guid] = TriggerData.Count;
            TriggerData.Add(evt);
            evt.dynamic = true;
        }
    }
}