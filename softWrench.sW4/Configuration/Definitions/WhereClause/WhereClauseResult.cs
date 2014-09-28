using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Configuration.Definitions.WhereClause {
    public class WhereClauseResult {
        public string Query { get; set; }
        public string ServiceName { get; set; }
        public string MethodName { get; set; }

        public override string ToString() {
            return string.Format("Query: {0}, ServiceName: {1}, MethodName: {2}", Query, ServiceName, MethodName);
        }

        public Boolean IsEmpty() {
            return string.IsNullOrEmpty(Query) && ServiceName == null;
        }
    }
}
