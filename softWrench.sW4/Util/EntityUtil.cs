using System;
using cts.commons.portable.Util;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Util {

    class EntityUtil {
        public static bool IsRelationshipNameEquals(string firstName, string secondName) {
            return GetRelationshipName(firstName) == GetRelationshipName(secondName);
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

        public static string GetQueryReplacingMarker(string query, string entityName) {
            return query.Replace("!@", entityName + ".");
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
