using System;
using cts.commons.portable.Util;

namespace softWrench.sW4.Data.Search {

    public class SearchParameter {

        public SearchParameter(string rawValue) {
            Refresh(rawValue);
        }

        public void Refresh(string newValue) {
            object value;
            bool searchFilter = false;
            SearchOperator = ParseSearchOperator(newValue, out value, out searchFilter);
            Value = value;
            FilterSearch = searchFilter;
        }

        private SearchOperator ParseSearchOperator(string rawValue, out object value, out bool searchFilter) {
            searchFilter = false;
            var searchOperator = SearchOperator.EQ;
            rawValue = HandleNullScenario(rawValue);
            if (rawValue.Contains(">=") && rawValue.Contains("<=")) {
                searchOperator = SearchOperator.BETWEEN;
            } else if (rawValue.StartsWith(">=")) {
                searchOperator = SearchOperator.GTE;
            } else if (rawValue.StartsWith("<=")) {
                searchOperator = SearchOperator.LTE;
            } else if (rawValue.StartsWith(">")) {
                searchOperator = SearchOperator.GT;
            } else if (rawValue.StartsWith("<")) {
                searchOperator = SearchOperator.LT;
            } else if (rawValue.StartsWith("!=")) {
                searchOperator = SearchOperator.NOTEQ;
            } else if (rawValue.StartsWith("!%")) {
                searchOperator = SearchOperator.NCONTAINS;
            } else if (rawValue.Contains(",")) {
                searchOperator = rawValue.StartsWith("%") || rawValue.EndsWith("%") ? SearchOperator.ORCONTAINS : SearchOperator.OR;
            } else if (rawValue.StartsWith("%")) {
                searchOperator = rawValue.EndsWith("%") ? SearchOperator.CONTAINS : SearchOperator.ENDWITH;
            } else if (rawValue.EndsWith("%")) {
                searchOperator = SearchOperator.STARTWITH;
            } else if (rawValue.Equals("!@BLANK", StringComparison.OrdinalIgnoreCase)) {
                searchOperator = SearchOperator.BLANK;
            }
            value = searchOperator.NormalizedValue(rawValue);
            if (!value.Equals(rawValue)) {
                searchFilter = true;
            }
            return searchOperator;
        }

        private string HandleNullScenario(string rawValue) {
            if (rawValue.StartsWith(SearchUtils.NullOrPrefix)) {
                NullOr = true;
                rawValue = rawValue.Substring(SearchUtils.NullOrPrefix.Length);
            } else if (rawValue.StartsWith("=" + SearchUtils.NullOrPrefix)) {
                NullOr = true;
                rawValue = rawValue.Substring(SearchUtils.NullOrPrefix.Length + 1);
            }
            return rawValue;
        }

        public SearchOperator SearchOperator {
            get; private set;
        }


        public bool IgnoreParameter {
            get; set;
        }

        /// <summary>
        /// if true this indicates a FilterSearch, that would need to be case-insensitive handled
        /// </summary>
        public bool FilterSearch {
            get; set;
        }

        public bool NullOr {
            get; set;
        }

        public object Value {
            get; set;
        }
        public bool IsList {
            get {
                return SearchOperator == SearchOperator.OR;
            }
        }

        public bool IsNumber {
            get; set;
        }

        public bool IsDate {
            get {
                return DateUtil.Parse(Value.ToString()) != null;
            }
        }

        public DateTime GetAsDate {
            get {
                DateTime? temp = DateUtil.Parse(Value.ToString());
                return temp != null ? temp.Value : new DateTime();
            }
        }

        public bool HasHour {
            get {
                Boolean hasHour = false;
                DateTime? temp = DateUtil.Parse(Value.ToString());
                if (temp != null) {
                    hasHour = (temp.Value.TimeOfDay.Ticks != 0);
                }
                return hasHour;
            }
        }

        public bool IsBlankNumber {
            get {
                return IsNumber && SearchOperator.BLANK == SearchOperator;
            }
        }

        public bool IsBlankDate {
            get {
                return IsDate && SearchOperator.BLANK == SearchOperator;
            }
        }

        public bool IsEqualOrNotEqual() {
            return SearchOperator == SearchOperator.EQ || SearchOperator == SearchOperator.NOTEQ;
        }

        public bool IsGTEOrLTE() {
            return SearchOperator == SearchOperator.GTE || SearchOperator == SearchOperator.LTE;
        }



        public bool IsGtOrGte() {
            return SearchOperator == SearchOperator.GT || SearchOperator == SearchOperator.GTE;
        }

        public bool IsLtOrLte() {
            return SearchOperator == SearchOperator.LT || SearchOperator == SearchOperator.LTE;
        }
    }


}
