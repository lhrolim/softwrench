using System.Collections.Generic;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    
    public interface IWhereBuilder {
        string BuildWhereClause(string entityName, QueryCacheKey.QueryMode queryMode, SearchRequestDto searchDto = null);
        IDictionary<string, object> GetParameters();
    }

}