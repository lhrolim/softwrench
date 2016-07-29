using System;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    public class BaseQueryBuilder {

        public static string GetServiceQuery(string query, string context = null) {
            return EntityUtil.GetServiceQuery(query, context);
        }
    }
}
