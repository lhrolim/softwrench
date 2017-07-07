using System;
using JetBrains.Annotations;

namespace softWrench.sW4.Configuration.Definitions.WhereClause {
    public class WhereClauseResult {

        [CanBeNull]
        public string Query { get; set; }
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public int AppliedProfileId { get; set; }

        public override string ToString() {
            return string.Format("Query: {0}, ServiceName: {1}, MethodName: {2}", Query, ServiceName, MethodName);
        }

        public bool IsEmpty() {
            return (Query== null || "".Equals(Query.Trim())) && ServiceName == null;
        }
    }
}
