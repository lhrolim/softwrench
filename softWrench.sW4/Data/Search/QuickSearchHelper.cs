using JetBrains.Annotations;

namespace softWrench.sW4.Data.Search {
    public class QuickSearchHelper {

        private const string QUICK_SEARCH_PARAM_NAME = "quicksearchstring";
        private const string QUICK_SEARCH_PARAM_VALUE_PATTERN = "%{0}%";
        private const string QUICK_SEARCH_PARAM_QUERY_PATTERN = "({0} like :" + QUICK_SEARCH_PARAM_NAME + ")";

        public static bool HasQuickSearchData([NotNull]SearchRequestDto dto){
            return !string.IsNullOrEmpty(dto.QuickSearchData);
        }

        public static string QuickSearchStatement([NotNull]string attribute) {
            return string.Format(QUICK_SEARCH_PARAM_QUERY_PATTERN, attribute);
        }

        public static string QuickSearchParamName {
            get { return QUICK_SEARCH_PARAM_NAME; }
        }

        public static string QuickSearchDataValue(string data) {
            return string.Format(QUICK_SEARCH_PARAM_VALUE_PATTERN, data);
        }
    }
}
