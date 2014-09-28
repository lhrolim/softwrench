using System;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Data.Search {
    public static class SearchOperatorExtensions {
        public static string OperatorPrefix(this SearchOperator searchOperator) {
            switch (searchOperator) {
                case SearchOperator.CONTAINS: return " like ";
                case SearchOperator.ENDWITH: return " like ";
                case SearchOperator.STARTWITH: return " like ";
                case SearchOperator.ORCONTAINS: return " like ";
                case SearchOperator.EQ: return " = ";
                case SearchOperator.GT: return " > ";
                case SearchOperator.GTE: return " >= ";
                case SearchOperator.LT: return " < ";
                case SearchOperator.LTE: return " <= ";
                case SearchOperator.NCONTAINS: return " not like ";
                case SearchOperator.NOTEQ: return " != ";
                case SearchOperator.OR: return " in ";
                case SearchOperator.BETWEEN: return " between ";
                default: return " = ";
            }
        }

        public static object NormalizedValue(this SearchOperator searchOperator, string rawValue) {
            if (rawValue == null) {
                return null;
            }
            rawValue = rawValue.Trim().ToUpper();

            switch (searchOperator) {
                case SearchOperator.CONTAINS:
                    if (!rawValue.StartsWith("%")) {
                        rawValue = "%" + rawValue;
                    }
                    if (!rawValue.EndsWith("%")) {
                        rawValue = rawValue + "%";
                    }
                    return rawValue;
                case SearchOperator.NCONTAINS:
                    rawValue = rawValue.Replace("!", "");
                    if (!rawValue.StartsWith("%")) {
                        rawValue = "%" + rawValue;
                    }
                    if (!rawValue.EndsWith("%")) {
                        rawValue = rawValue + "%";
                    }
                    return rawValue;
                case SearchOperator.ENDWITH:
                    if (!rawValue.StartsWith("%")) {
                        rawValue = "%" + rawValue;
                    }

                    return rawValue;
                case SearchOperator.STARTWITH:
                    if (!rawValue.EndsWith("%")) {
                        rawValue = rawValue + "%";
                    }
                    return rawValue;
                case SearchOperator.EQ: return rawValue.Replace("=", "");
                case SearchOperator.GT: return rawValue.Replace(">", "");
                case SearchOperator.GTE: return rawValue.Replace(">=", "");
                case SearchOperator.LT: return rawValue.Replace("<", "");
                case SearchOperator.LTE: return rawValue.Replace("<=", "");
                case SearchOperator.NOTEQ: return rawValue.Replace("!=", "");
                case SearchOperator.OR:
                case SearchOperator.ORCONTAINS:
                    rawValue = rawValue.Replace("=", "");
                    rawValue = rawValue.Replace("%", "");
                    var normalizedValue = rawValue.Split(',');
                    ICollection<string> result = new List<string>();
                    foreach (var value in normalizedValue) {
                        if (!string.IsNullOrEmpty(value)) {
                            result.Add(value);
                        }
                    }
                    return result;
                case SearchOperator.BETWEEN:
                    rawValue = rawValue.Replace(">=", "");
                    rawValue = rawValue.Replace("<=", "");
                    return rawValue;
                case SearchOperator.BLANK:
                    return "";
                default: return rawValue;
            }
        }
    }
}
