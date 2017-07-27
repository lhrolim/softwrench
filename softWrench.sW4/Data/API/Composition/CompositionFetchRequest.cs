
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;

namespace softWrench.sW4.Data.API.Composition {
    public class CompositionFetchRequest {

        public string Id { get; set; }
        public string UserId { get; set; }

        public ApplicationMetadataSchemaKey Key { get; set; }

        /// <summary>
        /// If this list is null every composition will be fetched
        /// </summary>
        [CanBeNull]
        public List<String> CompositionList { get; set; }

        /// <summary>
        /// Parameters to be propagated internally, that can be used by custom implementations.
        /// </summary>
        public IDictionary<string, object> ExtraParameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// DTO to paginate the composition request result
        /// </summary>
        public PaginatedSearchRequestDto PaginatedSearch { get; set; }


    }
}
