using System.Collections.Generic;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {
    public class AssociationPostFilterFunctionParameters {


        public AttributeHolder OriginalEntity { get; set; }
        public ISet<IAssociationOption> Options { get; set; }
        public ApplicationAssociationDefinition Association { get; set; }
    }
}
