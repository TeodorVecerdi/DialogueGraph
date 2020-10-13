using System;
using Newtonsoft.Json;

namespace Dlog {
    [Serializable]
    public class SerializedProperty {
        public string Type;
        public string Data;

        public SerializedProperty(AbstractProperty property) {
            Data = JsonConvert.SerializeObject(property, Formatting.None, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
            Type = property.GetType().FullName;
        }

        public AbstractProperty Deserialize() {
            var type = System.Type.GetType(Type);
            return (AbstractProperty) JsonConvert.DeserializeObject(Data, type, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
        }
    }
}