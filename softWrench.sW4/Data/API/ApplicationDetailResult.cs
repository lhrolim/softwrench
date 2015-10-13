using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API {
    public class ApplicationDetailResult : GenericResponseResult<DataMap>, IApplicationResponse {
        private readonly string _id;
        private readonly ApplicationSchemaDefinition _main;
        private readonly IDictionary<string, ApplicationCompositionSchema>
            _compositions = new Dictionary<string, ApplicationCompositionSchema>();

        private readonly IDictionary<string, BaseAssociationUpdateResult> _associationOptions
                    = new Dictionary<string, BaseAssociationUpdateResult>();


        private bool _allassociationsFetched;

        public bool AllassociationsFetched {
            get {
                if (!_allassociationsFetched && Schema.Properties.ContainsKey(ApplicationSchemaPropertiesCatalog.PreFetchAssociations)) {
                    return "#all".Equals(Schema.Properties[ApplicationSchemaPropertiesCatalog.PreFetchAssociations]);
                }
                return _allassociationsFetched;
            }
            set { _allassociationsFetched = value; }
        }


        public ApplicationDetailResult(DataMap dataMap, IDictionary<string, BaseAssociationUpdateResult> associationOptions,
            ApplicationSchemaDefinition main, IDictionary<string, ApplicationCompositionSchema> compositions, string id)
            : base(dataMap, null) {
            _compositions = compositions;
            _associationOptions = associationOptions;
            Schema = main;
            Schema.CompositionSchemas = compositions;
            _main = main;
            _id = id;
        }

        public IDictionary<string, BaseAssociationUpdateResult> AssociationOptions {
            get { return _associationOptions; }
        }

        public ApplicationSchemaDefinition Schema
        {
            get; set;
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
            get { return _compositions; }
        }

        public string Type {
            get { return GetType().Name; }
        }

        public string ApplicationName { get { return Schema.ApplicationName; } }

        public ApplicationSchemaDefinition Main {
            get { return _main; }
        }

    }
}
