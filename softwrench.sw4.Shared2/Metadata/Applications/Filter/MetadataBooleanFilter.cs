using System.ComponentModel;

namespace softwrench.sw4.Shared2.Metadata.Applications.Filter {

    public class MetadataBooleanFilter : BaseMetadataFilter {
        [DefaultValue(true)]
        public bool DefaultSelection {
            get; set;
        }

        public MetadataBooleanFilter(string attribute, string label, string icon, string position, string tooltip, string whereClause, bool defaultSelection) : base(attribute, label, icon, position, tooltip, whereClause) {
            DefaultSelection = defaultSelection;
        }
    }
}
