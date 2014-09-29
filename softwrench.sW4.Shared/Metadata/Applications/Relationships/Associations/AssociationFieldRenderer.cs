using System;
using System.ComponentModel;
using softwrench.sW4.Shared.Metadata.Applications.UI;

namespace softwrench.sW4.Shared.Metadata.Applications.Relationships.Associations {

    public class AssociationFieldRenderer : FieldRenderer {

        private const string WrongRenderer = "renderer {0} not found. Possible options are AUTOCOMPLETE,COMBO and LOOKUP";

        private AssociationRendererType EnumRendererType { get; set; }

        public AssociationFieldRenderer() {
            EnumRendererType = AssociationRendererType.COMBO;
            RendererType = EnumRendererType.ToString();
        }

        public AssociationFieldRenderer(string renderertype, string parameters, string targetName)
            : base(renderertype, parameters, targetName) {
            AssociationRendererType result;
            Enum.TryParse(renderertype, true, out result);
            EnumRendererType = result;

        }

        protected override void ValidateRendererType(String rendererType) {
            AssociationRendererType result;
            if (!Enum.TryParse(rendererType, true, out result)) {
                throw new InvalidEnumArgumentException(String.Format(WrongRenderer, rendererType));
            }
        }

        public enum AssociationRendererType {
            AUTOCOMPLETE, COMBO, LOOKUP
        }

        public bool IsLazyLoaded {
            get { return EnumRendererType != AssociationRendererType.COMBO; }
        }

    }
}
