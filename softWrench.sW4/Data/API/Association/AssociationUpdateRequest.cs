using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.API.Association {
    public class AssociationUpdateRequest : IDataRequest, IAssociationPrefetcherRequest {
        private Boolean _hasClientSearch = false;

        public string Id {
            get; set;
        }
        public string UserId {
            get; set;
        }

        // For self association updates
        public string AssociationFieldName {
            get; set;
        }
        public string AssociationApplication {
            get; set;
        }
        public ApplicationMetadataSchemaKey AssociationKey {
            get; set;
        }

        // For association options filtering
        public string ValueSearchString {
            get; set;
        }
        public string LabelSearchString {
            get; set;
        }

        /// <summary>
        /// to indicate whether this is coming from a search.
        /// //TODO: refactor this api splitting the methods
        /// </summary>
        public Boolean HasClientSearch {
            get {
                return _hasClientSearch || ValueSearchString != null || LabelSearchString != null;
            }
            set {
                _hasClientSearch = value;
            }
        }

        // For dependant association updates
        public string TriggerFieldName {
            get; set;
        }

        public ApplicationMetadataSchemaKey Key {
            get; set;
        }
        public String Title {
            get; set;
        }
        public IDictionary<string, object> CustomParameters {
            get; set;
        }
        public string CommandId {
            get; set;
        }

        public PaginatedSearchRequestDto SearchDTO {
            get; set;
        }

        public string AssociationsToFetch {
            get; set;
        }
        public bool IsShowMoreMode {
            get {
                return false;
            }
            set {
            }
        }
    }
}
