using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    class BaseQueryUtil {

        public const string LiteralDelimiter = "'";

        public static string AliasEntity(string entity, string alias) {
            var metadata = MetadataProvider.Entity(entity);
            var table = metadata.GetTableName();

            return string.Format("{0} as {1}", table, alias);
        }



        public static string QualifyAttribute(EntityMetadata entityMetadata, EntityAttribute attribute) {
            return attribute.IsAssociated
                ? attribute.Name
                : string.Format("{0}.{1}", entityMetadata.Name, attribute.Name);
        }

        public static String EvaluateServiceQuery(string query) {
            if (ApplicationConfiguration.IsUnitTest) {
                //TODO:fix this
                return query;
            }
            if (query.StartsWith("@")) {
                //removing leading @
                query = query.Substring(1);
                var split = query.Split('.');
                var ob = SimpleInjectorGenericFactory.Instance.GetObject<object>(split[0]);
                if (ob != null) {
                    var result = ReflectionUtil.Invoke(ob, split[1], new object[] { });
                    if (!(result is String)) {
                        throw ExceptionUtil.InvalidOperation("method need to return string for join whereclause");
                    }
                    query = result.ToString();
                }
            }
            return query;
        }


    }
}
