
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;

namespace softWrench.sW4.Data.API.Composition {
    public class PreFetchedCompositionFetchRequest : CompositionFetchRequest {

        public IReadOnlyList<AttributeHolder> PrefetchEntities { get; set; }

        public PreFetchedCompositionFetchRequest(IReadOnlyList<AttributeHolder> entities) {
            PrefetchEntities = entities;
        }
    }
}