using softWrench.sW4.Data.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {

    class CompositeWhereBuilder : IWhereBuilder {
        private const string And = " and ";

        private readonly IEnumerable<IWhereBuilder> _whereBuilders;

        public CompositeWhereBuilder(IEnumerable<IWhereBuilder> whereBuilders) {
            this._whereBuilders = whereBuilders;
        }

        public string BuildWhereClause(string entityName, QueryCacheKey.QueryMode queryMode, SearchRequestDto searchDto = null) {
            var sb = new StringBuilder();
            var firstMatch = true;
            foreach (var whereBuilder in _whereBuilders) {
                var result = whereBuilder.BuildWhereClause(entityName, queryMode, searchDto);
                if (String.IsNullOrWhiteSpace(result)) {
                    continue;
                }

                if (firstMatch) {
                    sb.Append(" where ");
                    firstMatch = false;
                }
                sb.Append("(").Append(result).Append(")").Append(And);
            }
            if (sb.Length > 0) {
                return sb.ToString(0, sb.Length - And.Length);
            }
            return null;
        }

        public IDictionary<string, object> GetParameters() {
            var parameters = new List<KeyValuePair<string, object>>();
            foreach (var whereBuilder in _whereBuilders) {
                var result = whereBuilder.GetParameters();
                if (result == null) continue;
                parameters.AddRange(result);
            }
            return parameters.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
        }
    }
}
