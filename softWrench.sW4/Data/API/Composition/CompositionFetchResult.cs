using System.Collections.Generic;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.API.Composition {
    public class CompositionFetchResult : GenericResponseResult<Dictionary<string, EntityRepository.SearchEntityResult>> {

        public Entity Cruddata { get; set; }


        public CompositionFetchResult(Dictionary<string, EntityRepository.SearchEntityResult> compositions, Entity cruddata)
            : base(compositions) {
            Cruddata = cruddata;
        }

    }
}