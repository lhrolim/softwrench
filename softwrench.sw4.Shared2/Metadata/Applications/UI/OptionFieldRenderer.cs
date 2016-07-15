using System;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Metadata.Applications.UI
{
    public class OptionFieldRenderer : FieldRenderer {

        private const string WrongRenderer = "renderer {0} not found. Possible options are COMBO, COMBODROPDOWN, CHECKBOX, AUTOCOMPLETECLIENT,MULTIPLE,MULTISELECTAUTOCOMPLETECLIENT and RADIO";

        private OptionRendererType EnumRendererType { get; set; }

        public OptionFieldRenderer() {
            EnumRendererType = OptionRendererType.COMBO;
            RendererType = EnumRendererType.ToString();
            Stereotype = null;
        }

        public OptionFieldRenderer(string renderertype, string parameters, string targetName, string stereotype = null)
            : base(renderertype, parameters, targetName, stereotype) {
            OptionRendererType result;
            Enum.TryParse(renderertype, true, out result);
            EnumRendererType = result;
        }

        protected override void ValidateRendererType(String rendererType) {
            OptionRendererType result;
            if (!Enum.TryParse(rendererType, true, out result)) {
                throw new InvalidOperationException(String.Format(WrongRenderer, rendererType));
            }
        }

        public enum OptionRendererType {
            COMBO, CHECKBOX, RADIO, COMBODROPDOWN, AUTOCOMPLETECLIENT, MULTIPLE, LOOKUP, MULTISELECTAUTOCOMPLETECLIENT
        }
    }
}
