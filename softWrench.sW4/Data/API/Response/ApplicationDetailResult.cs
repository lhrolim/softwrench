using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Association.SchemaLoading;
using softWrench.sW4.Metadata.Stereotypes.Schema;

namespace softWrench.sW4.Data.API.Response {
    public class ApplicationDetailResult : GenericResponseResult<DataMap>, IApplicationResponse {
        private readonly string _id;



        private readonly AssociationMainSchemaLoadResult _associationOptions;
                    



        public ApplicationDetailResult(DataMap dataMap, AssociationMainSchemaLoadResult associationOptions,
            ApplicationSchemaDefinition main, IDictionary<string, ApplicationCompositionSchema> compositions, string id)
            : base(dataMap, null) {
            
            _associationOptions = associationOptions;
            Schema = main;
            Schema.CompositionSchemas = compositions;
            _id = id;
        }

        public AssociationMainSchemaLoadResult AssociationOptions {
            get { return _associationOptions; }
        }


        public string CachedSchemaId { get; set; }

        public string Mode {
            get;
            set;
        }



        public string Id {
            get { return _id; }
        }

        public IDictionary<string, ApplicationCompositionSchema> Compositions {
            get { return Schema.CompositionSchemas; }
        }

        public string Type {
            get { return GetType().Name; }
        }

        public string ApplicationName { get { return Schema.ApplicationName; } }

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
    }
}
