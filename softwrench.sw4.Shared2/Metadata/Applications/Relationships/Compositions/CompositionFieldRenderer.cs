using System;
using System.ComponentModel;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions {
    
    public class CompositionFieldRenderer :FieldRenderer  {

        private const string WrongRenderer = "renderer {0} not found. Possible options are DEFAULT, TABLE,FILEEXPLORER, TABS and CUSTOM";

        private CompositionRendererType EnumRendererType { get; set; }

        public CompositionFieldRenderer() {
            EnumRendererType = CompositionRendererType.TABLE;
            RendererType = EnumRendererType.ToString();
            Stereotype = null;
        }

        public CompositionFieldRenderer(string renderertype, string parameters, string targetName, string stereotype = null)
            : base(renderertype, parameters, targetName, stereotype) {
                CompositionRendererType result;
            Enum.TryParse(renderertype, true, out result);
            EnumRendererType = result;
        }

        protected override void ValidateRendererType(String rendererType) {
            CompositionRendererType result;
            if (!Enum.TryParse(rendererType, true, out result)) {
                throw new InvalidOperationException(String.Format(WrongRenderer, rendererType));
            }
        }

        public enum CompositionRendererType {
            DEFAULT, TABLE, TABS, FILEEXPLORER, CUSTOM
        }
    
    }

}
