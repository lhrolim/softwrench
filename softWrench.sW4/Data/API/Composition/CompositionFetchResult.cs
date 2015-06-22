
using System.Collections.Generic;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.API.Composition {
    public class CompositionFetchResult : GenericResponseResult<Dictionary<string, EntityRepository.SearchEntityResult>> {

        public CompositionFetchResult(Dictionary<string, EntityRepository.SearchEntityResult> compositions)
            : base(compositions) {

        }

    }
}
