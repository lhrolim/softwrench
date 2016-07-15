﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    public class BaseQueryUtil {

        public const string LiteralDelimiter = "'";

        public static string AliasEntity(string entity, string alias) {
            var metadata = MetadataProvider.Entity(entity);
            var table = metadata.GetTableName();
            if (ApplicationConfiguration.IsOracle(DBType.Maximo)) {
                return string.Format("{0} {1}", table, alias);
            }
            return string.Format("{0} as {1}", table, alias);
        }

        public static string QualifyAttribute(EntityMetadata entityMetadata, EntityAttribute attribute) {
            return attribute.IsAssociated
                ? attribute.Name
                : string.Format("{0}.{1}", entityMetadata.Name, attribute.Name);
        }

        public static string GenerateInString(IEnumerable<string> items) {
            var sb = new StringBuilder();
            foreach (var item in items) {
                var escapedItem = item.Replace("'", "''");
                sb.Append("'").Append(escapedItem).Append("'");
                sb.Append(",");
            }
            return sb.ToString(0, sb.Length - 1);
        }

        public static string GenerateOrLikeString(string columnName, IEnumerable<string> items) {
            var sb = new StringBuilder();
            var enumerable = items as IList<string> ?? items.ToList();
            if (items == null || !enumerable.Any()) {
                return "1=1";
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

        public static string GenerateInString(IEnumerable<DataMap> items, string columnName = null) {
            var dataMaps = items as DataMap[] ?? items.ToArray();
            if (items == null || !dataMaps.Any()) {
                return null;
            }

            var sb = new StringBuilder();
            foreach (var item in dataMaps) {
                var id = columnName == null ? item.Id : item.GetAttribute(columnName);
                sb.Append("'").Append(id).Append("'");
                sb.Append(",");
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
