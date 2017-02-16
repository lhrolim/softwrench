using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;

namespace softWrench.sW4.Data.Search.QuickSearch {
    public class QuickSearchHelper : ISingletonComponent {

        private const string QUICK_SEARCH_PARAM_NAME = "quicksearchstring";
        private const string QUICK_SEARCH_PARAM_VALUE_PATTERN = "%{0}%";
        private const string QUICK_SEARCH_PARAM_QUERY_PATTERN = "(UPPER(COALESCE({0},'')) like :" + QUICK_SEARCH_PARAM_NAME + ")";

        public static bool HasQuickSearchData([NotNull]SearchRequestDto dto) {
            return dto.QuickSearchDTO != null;
        }

        public static string QuickSearchStatement([NotNull]string attribute, bool ignoreCoalesce = false) {
            return ignoreCoalesce 
                ? "({0} like :{1})".Fmt(attribute,QUICK_SEARCH_PARAM_NAME) 
                : string.Format(QUICK_SEARCH_PARAM_QUERY_PATTERN, attribute);
        }


        public static string QuickSearchParamName => QUICK_SEARCH_PARAM_NAME;

        public static string QuickSearchDataValue(string data) {
            return string.Format(QUICK_SEARCH_PARAM_VALUE_PATTERN, data).ToUpper();
        }


        /// <summary>
        /// Builds a whereclause statement for a quick search query. 
        /// The statement is applied to the attributes. 
        /// Optionally you can pass a context parameter to be used as the attributes's alias in the statement.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual string BuildOrWhereClause(IEnumerable<string> attributes, string context = null) {
            var attrs = attributes.ToList();

            /* not prefixing complete select statements */

            var attributesForStatement = context == null 
                ? attrs 
                : attrs.Where(atr => !atr.Contains("_.") &&  !IsSelectStatement(atr)).Select(a => context + "." + a).ToList();

            if (context != null) {
                attributesForStatement = attributesForStatement.Concat(attrs.Where(atr => atr.Contains("_.") || IsSelectStatement(atr)).ToList()).ToList();
            }

            // iterate filters and 'OR' the attributes
            return "(" + string.Join("OR", attributesForStatement.Select((item) => QuickSearchStatement(item))) + ")";
        }

        private static bool IsSelectStatement(string attribute) {
            var attr = attribute.TrimStart();
            return attr.StartsWith("select ", StringComparison.OrdinalIgnoreCase) || 
                attr.StartsWith("(select ", StringComparison.OrdinalIgnoreCase) || 
                attr.StartsWith("( select ", StringComparison.OrdinalIgnoreCase);
        }
    }
}
