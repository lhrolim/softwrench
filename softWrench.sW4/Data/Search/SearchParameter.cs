﻿using System;
using System.Globalization;
using System.Text;
using softWrench.sW4.Util;
using softwrench.sw4.Shared2.Util;

namespace softWrench.sW4.Data.Search {

    public class SearchParameter {

        public SearchParameter(string rawValue) {
            object value;
            SearchOperator = ParseSearchOperator(rawValue, out value);
            Value = value;
        }

        private SearchOperator ParseSearchOperator(string rawValue, out object value) {
            var searchOperator = SearchOperator.EQ;
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
                searchOperator = rawValue.StartsWith("%") && rawValue.EndsWith("%") ? SearchOperator.ORCONTAINS : SearchOperator.OR;
            } else if (rawValue.StartsWith("%")) {
                searchOperator = rawValue.EndsWith("%") ? SearchOperator.CONTAINS : SearchOperator.ENDWITH;
            } else if (rawValue.EndsWith("%")) {
                searchOperator = SearchOperator.STARTWITH;
            } else if (rawValue.Equals("IS NULL", StringComparison.OrdinalIgnoreCase)) {
                searchOperator = SearchOperator.BLANK;
            }
            value = searchOperator.NormalizedValue(rawValue);
            return searchOperator;
        }

        public SearchOperator SearchOperator { get; private set; }

        public object Value { get; private set; }
        public bool IsList { get { return SearchOperator == SearchOperator.OR; } }

        public bool IsNumber { get; set; }

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
