using System.Collections.Generic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    public class EntityWhereClauseBuilder : Basic.BaseQueryBuilder, IWhereBuilder {


        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            var entityMetadata = MetadataProvider.Entity(entityName);
            return !entityMetadata.HasWhereClause ? null : EntityUtil.GetQueryReplacingMarkers(entityMetadata.Schema.WhereClause, entityName);
        }

        public IDictionary<string, object> GetParameters() {
            return null;
        }
    }
}
