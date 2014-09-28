using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softWrench.Mobile.Metadata.Applications.UI {
    public sealed class TextWidget : TextWidgetDefinition,IWidget {


        public TextWidget(TextWidgetDefinition definition) {
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
