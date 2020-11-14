using Newtonsoft.Json;
using UnityEngine.UIElements;

namespace Dlog {
    public static class DlogExtensions {
        public static AbstractProperty Deserialize(this SerializedProperty property) {
            var type = System.Type.GetType(property.Type);
            return (AbstractProperty) JsonConvert.DeserializeObject(property.Data, type, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
        }

        public static void InjectCustomStyle(this DlogPort port) {
            var cap = port.Q("cap");
            var width = cap.style.width;
            width.value = new Length(8, LengthUnit.Pixel);
            cap.style.width = width;
            var height = cap.style.height;
            height.value = new Length(12, LengthUnit.Pixel);
            cap.style.height = height;

            // Border color
            var bLColor = cap.style.borderLeftColor;
            bLColor.value = port.portColor;
            cap.style.borderLeftColor = bLColor;
            
            var bTColor = cap.style.borderTopColor;
            bTColor.value = port.portColor;
            cap.style.borderTopColor = bTColor;
            
            var bRColor = cap.style.borderRightColor;
            bRColor.value = port.portColor;
            cap.style.borderRightColor = bRColor;
            
            var bBColor = cap.style.borderBottomColor;
            bBColor.value = port.portColor;
            cap.style.borderBottomColor = bBColor;
        }
    }
}