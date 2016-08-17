using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    public class MultiTenantCustomerWhereBuilder : IWhereBuilder {
        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            var entity = MetadataProvider.Entity(entityName);
            var property = MetadataProvider.GlobalProperty(SwConstants.MultiTenantPrefix);
            if (property == null) {
                return null;
            }

            if (entity.Schema.Attributes.Any(a => a.Name.Equals("pluspcustomer"))) {
                return string.Format("{0}.pluspcustomer like '{1}'", entityName, property);
            }

            if (entity.Schema.Attributes.Any(a => a.Name.Equals("pluspcustvendor"))) {
                return string.Format("{0}.pluspcustvendor like '{1}'", entityName, property);
            }

            if (entity.Schema.Attributes.Any(a => a.Name.Equals("pluspinsertcustomer"))) {
                return string.Format("{0}.pluspinsertcustomer like '{1}'", entityName, property);
            }

            return null;
        }

        public IDictionary<string, object> GetParameters() {
            return null;
        }
    }
}
