using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Metadata.Applications.Association;

namespace softWrench.sW4.Data.API.Association {
    public class SchemaAssociationPrefetcherRequest :IAssociationPrefetcherRequest{
        public string AssociationsToFetch { get {return AssociationHelper.AllButSchema; } set {} }
    }
}
