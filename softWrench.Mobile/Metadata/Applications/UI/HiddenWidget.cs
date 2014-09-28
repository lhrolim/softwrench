using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softWrench.Mobile.Metadata.Applications.UI {
    public sealed class HiddenWidget : HiddenWidgetDefinition,IWidget {

        private readonly HiddenWidgetDefinition _definition;

        public HiddenWidget(HiddenWidgetDefinition definition) {
            _definition = definition;
        }

        public string Type { get { return GetType().Name; } }


        public string Format(string value) {
            return value;
        }

        public bool Validate(string value, out string error) {
            error = null;
            return true;
        }
    }
}
