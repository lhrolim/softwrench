using System;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Metadata.Entities.Sliced {
    //TODO: review this class, after schemas have been migrated.
    class SlicedEntityMetadataKey {
        private readonly ApplicationMetadataSchemaKey _schema;
        private readonly string _entityName;
        private readonly string _applicationMetada;

        public SlicedEntityMetadataKey(ApplicationMetadataSchemaKey schema, string entityName, string applicationMetadata) {
            _schema = schema;
            _entityName = entityName;
            _applicationMetada = applicationMetadata;
        }

        public ApplicationMetadataSchemaKey Schema {
            get {
                return _schema;
            }
        }

        public string ApplicationMetada {
            get {
                return _applicationMetada;
            }
        }

        public string EntityName {
            get {
                return _entityName;
            }
        }

        protected bool Equals(SlicedEntityMetadataKey other) {
            return Equals(_schema, other._schema) && string.Equals(_entityName, other._entityName) && string.Equals(_applicationMetada, other._applicationMetada);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SlicedEntityMetadataKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (_schema != null ? _schema.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_entityName != null ? _entityName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_applicationMetada != null ? _applicationMetada.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
