using System.Collections.Generic;
using cts.commons.portable.Util;

namespace softWrench.sW4.Metadata.Properties {
    public class EnvironmentProperties {

        private readonly string _key;

        private readonly IDictionary<string, string> _properties;

        public EnvironmentProperties(string key, IDictionary<string, string> properties) {
            this._key = key;
            _properties = properties;
        }

        public string Key {
            get { return _key; }
        }

        public IDictionary<string, string> Properties {
            get { return _properties; }
        }

        public override string ToString() {
            return "Key: {0} , Properties: {1}".Fmt(Key,Properties);
        }
    }
}
