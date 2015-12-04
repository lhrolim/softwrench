using System;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    class BaseQueryBuilder {

        public static string GetServiceQuery(string query, string context = null) {
            //removing leading @
            query = query.Substring(1);
            var split = query.Split('.');
            var ob = SimpleInjectorGenericFactory.Instance.GetObject<object>(split[0]);
            if (ob == null) {
                throw new InvalidOperationException("cannot locate service {0}".Fmt(split[0]));
            }
            object result = null;
            if (context != null) {
                result = ReflectionUtil.Invoke(ob, split[1], new object[] { context });
            } else {
                result = ReflectionUtil.Invoke(ob, split[1], new object[] { });
            }
            if (!(result is String)) {
                throw ExceptionUtil.InvalidOperation("method need to return string for join whereclause");
            }
            query = result.ToString();
            return query;
        }
    }
}
