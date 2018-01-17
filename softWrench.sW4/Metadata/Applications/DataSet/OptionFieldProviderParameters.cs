using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Metadata.Applications.DataSet {

    /// <summary>
    /// Parameters to be used on a method to be declared on the providerattribute from the metadata layer. Should return IEnumerable&lt;IAssociationOption&gt; 
    /// </summary>
    public class OptionFieldProviderParameters {

        public ApplicationMetadata ApplicationMetadata { get; set; }

        /// <summary>
        /// the datamap of the main entity present at screen
        /// </summary>
        public AttributeHolder OriginalEntity { get; set; }

        public OptionField OptionField { get; set; }
        
    }
}
