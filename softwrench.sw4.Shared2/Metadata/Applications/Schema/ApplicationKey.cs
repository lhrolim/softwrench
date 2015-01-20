using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema {
    public class ApplicationKey {


        public ApplicationKey() { }

        public ApplicationKey(ApplicationSchemaDefinition schema) {
            ApplicationName = schema.ApplicationName;
            SchemaKey = schema.GetSchemaKey();
        }

        public ApplicationKey(Tuple<String,ApplicationMetadataSchemaKey> tuple) {
            ApplicationName = tuple.Item1;
            SchemaKey = tuple.Item2;
        }


        public string ApplicationName { get; set; }
        public ApplicationMetadataSchemaKey SchemaKey { get; set; }

        protected bool Equals(ApplicationKey other) {
            return string.Equals(ApplicationName, other.ApplicationName) && Equals(SchemaKey, other.SchemaKey);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((ApplicationName != null ? ApplicationName.GetHashCode() : 0) * 397) ^ (SchemaKey != null ? SchemaKey.GetHashCode() : 0);
            }
        }
    }
}
