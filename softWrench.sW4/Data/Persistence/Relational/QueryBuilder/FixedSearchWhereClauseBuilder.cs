using System.Collections.Generic;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    public class FixedSearchWhereClauseBuilder : IWhereBuilder {
        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
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
