using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.Persistence.WS.Rest {
    public class RestIntegrationObjectWrapper : IRestObjectWrapper {

        private readonly IDictionary<string, object> _entries = new Dictionary<string, object>();

        public IDictionary<string, object> Entries {
            get { 
                return _entries;
            }
        }

        public void AddEntry(string key, object value) {
            if (!_entries.ContainsKey(key)){
                _entries.Add(key, value);
            }
        }

        
        public bool HasNonInlineComposition {
            get {
                return _entries.Values.Any(a => a is RestComposedData && !((RestComposedData)a).Inline);
            }
        }

    }
}
