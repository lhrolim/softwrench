using System.Collections.Generic;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    
    public interface IWhereBuilder {
        string BuildWhereClause(string entityName, SearchRequestDto searchDto = null);
        IDictionary<string, object> GetParameters();
    }

}