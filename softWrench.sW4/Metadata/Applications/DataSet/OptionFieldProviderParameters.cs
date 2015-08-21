using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class OptionFieldProviderParameters {

        public ApplicationMetadata ApplicationMetadata { get; set; }

        public AttributeHolder OriginalEntity { get; set; }

        public OptionField OptionField { get; set; }
        
    }
}
