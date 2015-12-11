using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.API.Association {
    public class InlineCompositionAssociationPrefetcherRequest : IAssociationPrefetcherRequest {

        public InlineCompositionAssociationPrefetcherRequest(IAssociationPrefetcherRequest mainSchemaRequest, string compositionKey) {
            AssociationsToFetch = mainSchemaRequest.AssociationsToFetch;
            IsShowMoreMode = mainSchemaRequest.IsShowMoreMode;
            SchemaIdentifier = compositionKey;
        }


        public string SchemaIdentifier {
            get; set;
        }

        public string AssociationsToFetch {
            get; set;
        }
        public bool IsShowMoreMode {
            get; set;
        }
    }
}
