using System;
using Newtonsoft.Json;

namespace DialogueGraph {
    [Serializable]
    public class SerializedProperty {
        public string Type;
        public string Data;

        public SerializedProperty(AbstractProperty property) {
            Data = JsonConvert.SerializeObject(property, Formatting.None, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
            Type = property.GetType().FullName;
        }
    }
}