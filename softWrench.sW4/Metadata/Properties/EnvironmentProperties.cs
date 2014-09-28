using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Metadata.Properties {
    public class EnvironmentProperties {

        private readonly string key;

        private readonly IDictionary<string, string> _properties = new Dictionary<string, string>();

        public EnvironmentProperties(string key, IDictionary<string, string> properties) {
            this.key = key;
            _properties = properties;
        }

        public string Key {
            get { return key; }
        }

        public IDictionary<string, string> Properties {
            get { return _properties; }
        }
    }
}
