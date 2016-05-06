using System.Collections.Generic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    public class EntityWhereClauseBuilder : Basic.BaseQueryBuilder, IWhereBuilder {


        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            var entityMetadata = MetadataProvider.Entity(entityName);
            if (!entityMetadata.HasWhereClause) {
                return null;
            }

            //double check
            var queryReplacingMarker = EntityUtil.GetQueryReplacingMarker(entityMetadata.Schema.WhereClause, entityName);
            if (queryReplacingMarker != null && queryReplacingMarker.StartsWith("@")) {
                queryReplacingMarker = GetServiceQuery(queryReplacingMarker);
            }
            return queryReplacingMarker;
        }

        public IDictionary<string, object> GetParameters() {
            return null;
        }
    }
}
