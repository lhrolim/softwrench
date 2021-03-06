﻿using System;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Metadata.Entities.Sliced {
    //TODO: review this class, after schemas have been migrated.
    class SlicedEntityMetadataKey {
        private readonly ApplicationMetadataSchemaKey _schema;
        private readonly String _entityName;

        public SlicedEntityMetadataKey(ApplicationMetadataSchemaKey schema, string entityName) {
            _schema = schema;
            this._entityName = entityName;
        }

        public ApplicationMetadataSchemaKey Schema {
            get { return _schema; }
        }

        public string EntityName {
            get { return _entityName; }
        }

        protected bool Equals(SlicedEntityMetadataKey other) {
            return Equals(_schema, other._schema) && string.Equals(_entityName, other._entityName);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SlicedEntityMetadataKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((_schema != null ? _schema.GetHashCode() : 0) * 397) ^ (_entityName != null ? _entityName.GetHashCode() : 0);
            }
        }
    }
}
