using System.Collections.Generic;
using softWrench.sW4.Metadata.Stereotypes.Schema;

namespace softWrench.sW4.Metadata.Stereotypes {

    abstract class ASchemaStereotype : IStereotype {

        public const string NextSchemaId = ApplicationSchemaPropertiesCatalog.RoutingNextSchemaId;
        public const string NextSchemaMode = "nextschema.schemamode";

        protected abstract IDictionary<string, string> DefaultValues();

        private readonly IDictionary<string, string> _values = new Dictionary<string, string>();

        protected ASchemaStereotype() {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            var defaultValues = DefaultValues();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
            foreach (var defaultValue in defaultValues) {
                var customClientValue = GetCustomClientValue(defaultValue);
                _values.Add(defaultValue.Key, customClientValue);
            }
        }

        private static string GetCustomClientValue(KeyValuePair<string, string> defaultValue) {
            var globalProperties = MetadataProvider.GlobalProperties.Properties;
            string globalValue;
            if (globalProperties.TryGetValue(defaultValue.Key, out globalValue)) {
                return globalValue;
            }
            return defaultValue.Value;
        }

        public string LookupValue(string key) {
            return _values.ContainsKey(key) ? _values[key] : null;
        }

        public IDictionary<string, string> StereotypeProperties() {
            return _values;
        }

        public void Merge(IStereotype stereotype) {

        }
    }
}
