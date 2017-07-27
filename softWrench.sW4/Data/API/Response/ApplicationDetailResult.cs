using System.Collections.Generic;
using JetBrains.Annotations;
using softwrench.sw4.api.classes.integration;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Association.SchemaLoading;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata.Stereotypes.Schema;

namespace softWrench.sW4.Data.API.Response {
    public class ApplicationDetailResult : GenericResponseResult<DataMap>, IApplicationResponse {
        public bool FullRefresh { get; set; } = false;


        public ApplicationDetailResult(DataMap dataMap, AssociationMainSchemaLoadResult associationOptions,
            ApplicationSchemaDefinition main, [CanBeNull]IDictionary<string, ApplicationCompositionSchema> compositions, string id)
            : base(dataMap, null) {
            
            AssociationOptions = associationOptions;
            Schema = main;
            Schema.CompositionSchemas = compositions;
            Id = id;
        }

        public AssociationMainSchemaLoadResult AssociationOptions { get; set; }


        public string CachedSchemaId { get; set; }

        public string Mode {
            get;
            set;
        }



        public string Id { get; }

        public IErrorDto WarningDto { get; set; }

        public IDictionary<string, ApplicationCompositionSchema> Compositions => Schema.CompositionSchemas;

        public IEnumerable<KeyValuePair<string, EntityRepository.SearchEntityResult>> EagerCompositionResult { get; set; }
        

        public string ApplicationName => Schema.ApplicationName;

        public ApplicationSchemaDefinition Schema { get; set; }

        private bool _allassociationsFetched;

        public bool AllAssociationsFetched {
            get {
                if (!_allassociationsFetched && Schema.Properties.ContainsKey(ApplicationSchemaPropertiesCatalog.PreFetchAssociations)) {
                    return "#all".Equals(Schema.Properties[ApplicationSchemaPropertiesCatalog.PreFetchAssociations]);
                }
                return _allassociationsFetched;
            }
            set { _allassociationsFetched = value; }
        }

        public bool ShouldSerializeSchema() {
            return (CachedSchemaId == null);
        }
    }
}
