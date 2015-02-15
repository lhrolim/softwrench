using System;
using System.ComponentModel;
using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations {

    public class AssociationFieldRenderer : FieldRenderer {

        private const string WrongRenderer = "renderer {0} not found. Possible options are AUTOCOMPLETECLIENT, AUTOCOMPLETESERVER, COMBO and LOOKUP";

        private AssociationRendererType EnumRendererType { get; set; }

        public AssociationFieldRenderer() {
            EnumRendererType = AssociationRendererType.AUTOCOMPLETECLIENT;
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
                throw new InvalidOperationException(String.Format(WrongRenderer, rendererType));
            }
        }

        public enum AssociationRendererType {
            AUTOCOMPLETECLIENT, AUTOCOMPLETESERVER, COMBO, LOOKUP, CUSTOM, COMBODROPDOWN
        }

        public bool IsLazyLoaded {
            get
            {
                return EnumRendererType != AssociationRendererType.AUTOCOMPLETECLIENT &&
                       EnumRendererType != AssociationRendererType.COMBO &&
                       EnumRendererType != AssociationRendererType.COMBODROPDOWN &&
                       EnumRendererType != AssociationRendererType.CUSTOM;
            }
        }

        public bool IsPaginated {
            get { return EnumRendererType == AssociationRendererType.LOOKUP; }
        }
    }
}
