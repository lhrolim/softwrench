using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.API.Association {
    public class AssociationUpdateRequest : IDataRequest, IAssociationPrefetcherRequest
    {

        public string Id { get; set; }

        // For self association updates
        public string AssociationFieldName { get; set; }
        public string AssociationApplication { get; set; }
        public ApplicationMetadataSchemaKey AssociationKey { get; set; }

        // For association options filtering
        public string ValueSearchString { get; set; }
        public string LabelSearchString { get; set; }

        // For dependant association updates
        public string TriggerFieldName { get; set; }

        public ApplicationMetadataSchemaKey Key { get; set; }
        public String Title { get; set; }
        public IDictionary<string, string> CustomParameters { get; set; }
        public string CommandId { get; set; }

        public PaginatedSearchRequestDto SearchDTO { get; set; }

        public Boolean HasClientSearch() {
            return ValueSearchString != null || LabelSearchString != null;
        }

        public string AssociationsToFetch { get; set; }
    }
}
