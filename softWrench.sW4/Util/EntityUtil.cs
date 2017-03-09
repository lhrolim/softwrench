using System;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Util {

    class EntityUtil {
        public static bool IsRelationshipNameEquals(string firstName, string secondName) {
            var first = GetRelationshipName(firstName);
            var second = GetRelationshipName(secondName);
            if (first == null) {
                return second == null;
            }
            return first.EqualsIc(second);
        }

        public static string GetRelationshipName(string attribute) {
            if (attribute == null) {
                return null;
            }

            var idx = attribute.IndexOf(".", System.StringComparison.Ordinal);
            if (idx == -1) {
                return attribute.EndsWith("_") ? attribute.Trim('\'') : attribute.Trim('\'') + "_"; ;
            }
            string relationship = attribute.Substring(0, idx);
            return relationship.EndsWith("_") ? relationship.Trim('\'') : relationship.Trim('\'') + "_";
        }

        public static string GetRelationshipName(string attribute, out string resultAttributeName) {
            var idx = attribute.IndexOf(".", System.StringComparison.Ordinal);
            if (idx == -1) {
                resultAttributeName = null;
                return attribute.EndsWith("_") ? attribute.Trim('\'') : attribute.Trim('\'') + "_"; ;
            }
            string relationship = attribute.Substring(0, idx);
            var lastUnderscoreIdx = relationship.LastIndexOf("_", System.StringComparison.Ordinal);
            var firstUnderscoreIdx = relationship.IndexOf("_", System.StringComparison.Ordinal);
            if (firstUnderscoreIdx != lastUnderscoreIdx && lastUnderscoreIdx != -1) {
                //this means that we have two or more levels
                resultAttributeName = attribute.Substring(firstUnderscoreIdx + 1);
                return relationship.Substring(0, firstUnderscoreIdx + 1).Trim('\'');

            }
            resultAttributeName = attribute.Substring(idx + 1);
            return relationship.EndsWith("_") ? relationship.Trim('\'') : relationship.Trim('\'') + "_";
        }

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

        public static string GetQueryReplacingMarkers(string query, string entityName, string fromValue = null) {
            var queryReplacingMarker = query.Replace("!@", entityName + ".");

            if (fromValue != null) {
                queryReplacingMarker = queryReplacingMarker.Replace("@from", fromValue);
            }

            if (queryReplacingMarker.StartsWith("@")) {
                queryReplacingMarker = GetServiceQuery(queryReplacingMarker);
            } else {
                var user = SecurityFacade.CurrentUser();
                queryReplacingMarker = DefaultValuesBuilder.ConvertAllValues(queryReplacingMarker, user);
            }
            return queryReplacingMarker;
        }

        public static string EvaluateQuery(string query, AttributeHolder holder) {
            if (!query.Contains("!@")) {
                return query;
            }

            var substrings = StringUtil.GetSubStrings(query, "!@");

            foreach (var keyword in substrings) {
                var value = holder.GetAttribute(keyword.Substring(2));

                if (value == null) {
                    //TODO: improve this to allow spaces, using a regex instead
                    query = query.Replace(" = " + keyword, " is null ");
                } else if (value is string) {
                    query = query.Replace("'" + keyword + "'", "'" + value + "'").Replace(keyword, "'" + value + "'");
                } else if (value is Int32) {
                    query = query.Replace("'" + keyword + "'", "" + value).Replace(keyword, "" + value);
                }
            }

            return query;


        }


        public static string GetApplicationName(string relationship) {
            if (!relationship.EndsWith("_")) {
                return relationship;
            }
            return relationship.Substring(0, relationship.Length - 1);
        }


    }
}
