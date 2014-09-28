using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Metadata.Entities {
    class EntityQueries {
        private IDictionary<string, string> _queries = new Dictionary<string, string>();

        public EntityQueries(Dictionary<string, string> queries) {
            _queries = queries;
        }


        public IDictionary<string, string> Queries {
            get { return _queries; }
            set { _queries = value; }
        }

        public string GetQuery(string key, bool throwException) {
            string result;
            var realKey =key.Substring("ref:".Length);
            if (!_queries.TryGetValue(realKey, out result)) {
                if (throwException) {
                    throw new InvalidOperationException(String.Format("Query {0} could not be located, please check your metadata", realKey));
                }
                return null;
            }
            return result;
        }
    }
}
