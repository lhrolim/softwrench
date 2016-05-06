using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    public class MultiTenantCustomerWhereBuilder : IWhereBuilder {
        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            var entity = MetadataProvider.Entity(entityName);
            if (!entity.Schema.Attributes.Any(a => a.Name.Equals("pluspcustomer"))) {
                return null;
            }
            var property = MetadataProvider.GlobalProperty("multitenantprefix");
            if (property == null) {
                return null;
            }
            return String.Format("{0}.pluspcustomer like '{1}'",entityName, property);


        }

        public IDictionary<string, object> GetParameters() {
            return null;
        }
    }
}
