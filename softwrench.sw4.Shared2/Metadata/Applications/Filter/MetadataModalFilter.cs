using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sw4.Shared2.Metadata.Applications.Filter {

    public class MetadataModalFilter : BaseMetadataFilter {

        public string TargetSchemaId {
            get; set;
        }

        public string Service {
            get; set;
        }

        public MetadataModalFilter(string attribute, string label, string icon, string position, string tooltip, string whereClause, string targetSchemaId, string service) : base(attribute, label, icon, position, tooltip, whereClause) {
            TargetSchemaId = targetSchemaId;
            Service = service;
        }
    }
}
