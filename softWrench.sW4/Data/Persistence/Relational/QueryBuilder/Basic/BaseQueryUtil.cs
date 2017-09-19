using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using cts.commons.Util;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Util;
using WebGrease.Css.Extensions;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    public class BaseQueryUtil {

        public const string LiteralDelimiter = "'";

        public static string AliasEntity(string entity, string alias) {
            var metadata = MetadataProvider.Entity(entity);
            var table = metadata.GetTableName();
            if (ApplicationConfiguration.IsOracle(DBType.Maximo)) {
                return $"{table} {alias}";
            }
            return $"{table} as {alias}";
        }

        public static string QualifyAttribute(EntityMetadata entityMetadata, EntityAttribute attribute) {
            return attribute.IsAssociated
                ? attribute.Name
                : $"{entityMetadata.Name}.{attribute.Name}";
        }

        public static string GenerateInString(IEnumerable<string> items) {
            if (items == null || !items.Any()) {
                return "";
            }

            var enumerable = items as ISet<string> ?? items.ToHashSet();
            Validate.NotEmpty(enumerable, "items");

            var sb = new StringBuilder();
            foreach (var item in enumerable) {
                var escapedItem = item.Replace("'", "''");
                sb.Append("'").Append(escapedItem).Append("'");
                sb.Append(",");
            }
            return sb.ToString(0, sb.Length - 1);
        }

        public static string GenerateOrLikeString(string columnName, IEnumerable<string> items, bool bringNoneIfEmpty = false) {
            var sb = new StringBuilder();
            var enumerable = items as ISet<string> ?? items.ToHashSet();
            if (items == null || !enumerable.Any()) {
                return bringNoneIfEmpty ? "1!=1" : "1=1";
            }

            foreach (var item in enumerable) {
                var escapedItem = item.Replace("'", "''");
                if (escapedItem.Contains("%")) {
                    sb.AppendFormat(" {0} like '{1}'", columnName, escapedItem);
                } else {
                    sb.AppendFormat(" {0} = '{1}'", columnName, escapedItem);
                }
                sb.Append(" or ");
            }
            return sb.ToString(0, sb.Length - 4);
        }

        private ExpandoObject FromDictionary(IDictionary<string, object> dict) {
            var parameters = new ExpandoObject();
            var parameterCollection = (ICollection<KeyValuePair<string, object>>)parameters;
            dict.ForEach(e => parameterCollection.Add(e));
            return parameters;
        }

        public static string GenerateInString(IEnumerable<DataMap> items, string columnName = null) {

            var dataMaps = items as DataMap[] ?? items.ToArray();
            if (items == null || !dataMaps.Any()) {
                return null;
            }
            var usedItems = new HashSet<object>();
            var sb = new StringBuilder();
            foreach (var item in dataMaps) {
                var id = columnName == null ? item.Id : item.GetAttribute(columnName);
                if (!usedItems.Contains(id)) {
                    sb.Append("'").Append(id).Append("'");
                    sb.Append(",");
                }
                usedItems.Add(id);
            }
            return sb.ToString(0, sb.Length - 1);
        }

        public static string GenerateInString(IEnumerable<AttributeHolder> items, [NotNull]string columnName) {
            var dataMaps = items as DataMap[] ?? items.ToArray();
            if (items == null || !dataMaps.Any()) {
                return null;
            }
            var usedItems = new HashSet<object>();
            var sb = new StringBuilder();
            foreach (var item in dataMaps) {
                var id = item.GetAttribute(columnName);
                if (!usedItems.Contains(id)) {
                    sb.Append("'").Append(id).Append("'");
                    sb.Append(",");
                }
                usedItems.Add(id);
            }
            return sb.ToString(0, sb.Length - 1);
        }


        public static string ParseAttributeForQuery(EntityMetadata entityMetadata, string alias, EntityAttribute attribute, string context = null) {
            var contextToUse = context ?? entityMetadata.Name;
            var query = attribute.Query;
            if (attribute.Query == null) {
                return alias + "." + attribute.Name;
            }
            if (query.StartsWith("@")) {
                return BaseQueryBuilder.GetServiceQuery(query, contextToUse);
            }
            return attribute.GetQueryReplacingMarkers(contextToUse);
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
