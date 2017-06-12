using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    class FixedSearchWhereClauseBuilder : IWhereBuilder {
        public string BuildWhereClause(string entityName, QueryCacheKey.QueryMode queryMode, SearchRequestDto searchDto = null) {
            if (searchDto != null && searchDto.FilterFixedWhereClause != null) {
                return searchDto.FilterFixedWhereClause;
            }
            return null;
        }

        public IDictionary<string, object> GetParameters() {
            return null;
        }
    }
}
