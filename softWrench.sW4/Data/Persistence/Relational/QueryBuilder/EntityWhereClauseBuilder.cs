using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    class EntityWhereClauseBuilder : IWhereBuilder {


        public string BuildWhereClause(string entityName, QueryCacheKey.QueryMode queryMode, SearchRequestDto searchDto = null) {
            var entityMetadata = MetadataProvider.Entity(entityName);
            //double check
            return entityMetadata.HasWhereClause ? entityMetadata.Schema.WhereClause : null;
        }

        public IDictionary<string, object> GetParameters() {
            return null;
        }
    }
}
