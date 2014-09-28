using System;
using System.ComponentModel;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Applications.Association;

namespace softWrench.sW4.Metadata.Applications.Relationship.Composition {

    public class CompositionFieldRenderer :FieldRenderer{

        private const string WrongRenderer = "renderer {0} not found. Possible options are TABLE,TABS,MultiInline";

        private CompositionRendererType EnumRendererType { get; set; }

           public CompositionFieldRenderer() {
            EnumRendererType = CompositionFieldRenderer.CompositionRendererType.TABLE;
            RendererType = EnumRendererType.ToString();
        }

        public CompositionFieldRenderer([NotNull]string renderertype, string parameters, string targetName)
            : base(renderertype, parameters, targetName) {
            CompositionRendererType result;
            Enum.TryParse(renderertype, true, out result);
            EnumRendererType = result;

        }

        protected override void ValidateRendererType(String rendererType) {
            AssociationFieldRenderer.AssociationRendererType result;
            if (!Enum.TryParse(rendererType, true, out result)) {
                throw new InvalidEnumArgumentException(String.Format(WrongRenderer, rendererType));
            }
        }


        internal enum CompositionRendererType
        {
            TABLE,TABS,MultiInline,
        }

    }
}
