using System;

namespace Dlog {
    [Serializable]
    public class CheckProperty : AbstractProperty {
        public CheckProperty() {
            DisplayName = "Check";
            Type = PropertyType.Check;
        }

        public override AbstractProperty Copy() {
            return new CheckProperty {
                DisplayName = DisplayName,
                Hidden = Hidden
            };
        }
    }
}