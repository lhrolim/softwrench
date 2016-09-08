using System;
using System.ComponentModel;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations {

    public class AssociationFieldRenderer : FieldRenderer {

        private const string WrongRenderer = "renderer {0} not found. Possible options are AUTOCOMPLETECLIENT, MULTISELECTAUTOCOMPLETECLIENT, AUTOCOMPLETESERVER, COMBO, LOOKUP, MODAL, CUSTOM and COMBODROPDOWN";

        private AssociationRendererType EnumRendererType { get; set; }

        public AssociationFieldRenderer() {
            EnumRendererType = AssociationRendererType.LOOKUP;
            RendererType = EnumRendererType.ToString();
            Stereotype = null;
        }

        public AssociationFieldRenderer(string renderertype, string parameters, string targetName, string stereotype = null)
            : base(renderertype, parameters, targetName, stereotype) {
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
            AUTOCOMPLETECLIENT, MULTISELECTAUTOCOMPLETECLIENT, AUTOCOMPLETESERVER, COMBO, LOOKUP, CUSTOM, COMBODROPDOWN, MODAL
        }

        public bool IsLazyLoaded {
            get { return EnumRendererType != AssociationRendererType.AUTOCOMPLETECLIENT && EnumRendererType != AssociationRendererType.MULTISELECTAUTOCOMPLETECLIENT && EnumRendererType != AssociationRendererType.COMBO && EnumRendererType != AssociationRendererType.COMBODROPDOWN; }
        }

        public bool IsPaginated {
            get { return EnumRendererType == AssociationRendererType.LOOKUP; }
        }
    }
}
