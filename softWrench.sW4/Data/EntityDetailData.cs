using softwrench.sW4.Shared2.Metadata;

namespace softWrench.sW4.Data {
    class EntityDetailData {
        private readonly string _entityName;
        private readonly string _idField;
        private readonly string _idValue;
        private readonly string _projectionFields=null;

        public EntityDetailData(CompleteApplicationMetadataDefinition application, string idValue = null) {
            this._idValue = idValue;
            this._idField = application.IdFieldName;
            this._entityName = application.Entity;

            //TODO: I've hacked this line to use the web schema
            //because currently we don't have an ENTITY METADATA
            //concept. As soons as we have it, the list of ATTRIBUTES
            //should be used instead of the list of FORM FIELDS.
//            this._projectionFields = string.Join(",", application.WebSchema.Fields.Select(f => f.Attribute)); 
        }

        public string EntityName {
            get { return _entityName; }
        }

        public string IdField {
            get { return _idField; }
        }

        public string IdValue {
            get { return _idValue; }
        }

        public string ProjectionFields {
            get { return _projectionFields; }
        }
    }
}
