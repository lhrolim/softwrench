using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace softWrench.sW4.Data.Search {
    public class QuickSearchHelper {

        private const string QUICK_SEARCH_PARAM_NAME = "quicksearchstring";
        private const string QUICK_SEARCH_PARAM_VALUE_PATTERN = "%{0}%";
        private const string QUICK_SEARCH_PARAM_QUERY_PATTERN = "(UPPER(COALESCE({0},'')) like :" + QUICK_SEARCH_PARAM_NAME + ")";

        public static bool HasQuickSearchData([NotNull]SearchRequestDto dto) {
            return !string.IsNullOrEmpty(dto.QuickSearchData);
        }

        public static string QuickSearchStatement([NotNull]string attribute) {
            return string.Format(QUICK_SEARCH_PARAM_QUERY_PATTERN, attribute);
        }

        public static string QuickSearchParamName {
            get { return QUICK_SEARCH_PARAM_NAME; }
        }

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
        public static string BuildOrWhereClause(IEnumerable<string> attributes, string context = null) {
            var attributesForStatement = context == null ? attributes : attributes.Select(a => context + "." + a);
            // iterate filters and 'OR' the attributes
            return "(" + string.Join("OR", attributesForStatement.Select(QuickSearchStatement)) + ")";
        }
    }
}
