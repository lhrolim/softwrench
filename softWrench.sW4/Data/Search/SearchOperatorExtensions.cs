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

        public static object NormalizedValue(this SearchOperator searchOperator, string rawValue)
        {
            string rawValue1 = rawValue;
            if (rawValue1 == null) {
                return null;
            }
            var originalValue = rawValue1;
            rawValue1 = rawValue1.Trim().ToUpper();
            switch (searchOperator) {
                case SearchOperator.CONTAINS:
                    if (!rawValue1.StartsWith("%")) {
                        rawValue1 = "%" + rawValue1;
                    }
                    if (!rawValue1.EndsWith("%")) {
                        rawValue1 = rawValue1 + "%";
                    }
                    return rawValue1;
                case SearchOperator.NCONTAINS:
                    rawValue1 = rawValue1.Replace("!", "");
                    if (!rawValue1.StartsWith("%")) {
                        rawValue1 = "%" + rawValue1;
                    }
                    if (!rawValue1.EndsWith("%")) {
                        rawValue1 = rawValue1 + "%";
                    }
                    return rawValue1;
                case SearchOperator.ENDWITH:
                    if (!rawValue1.StartsWith("%")) {
                        rawValue1 = "%" + rawValue1;
                    }

                    return rawValue1;
                case SearchOperator.STARTWITH:
                    if (!rawValue1.EndsWith("%")) {
                        rawValue1 = rawValue1 + "%";
                    }
                    return rawValue1;
                case SearchOperator.EQ:
                    if (rawValue1.Contains("="))
                    {
                        return rawValue1.Replace("=", "");    
                    }
                    return originalValue.Replace("=", "");
                case SearchOperator.GT:
                    return rawValue1.Replace(">", "");
                case SearchOperator.GTE:
                    return rawValue1.Replace(">=", "");
                case SearchOperator.LT:
                    return rawValue1.Replace("<", "");
                case SearchOperator.LTE:
                    return rawValue1.Replace("<=", "");
                case SearchOperator.NOTEQ:
                    return rawValue1.Replace("!=", "");
                case SearchOperator.OR:
                case SearchOperator.ORCONTAINS:
                    rawValue1 = rawValue1.Replace("=", "");
                    rawValue1 = rawValue1.Replace("%", "");
                    var normalizedValue = rawValue1.Split(',');
                    ICollection<string> result = new List<string>();
                    foreach (var value in normalizedValue) {
                        if (!string.IsNullOrEmpty(value)) {
                            result.Add(value);
                        }
                    }
                    return result;
                case SearchOperator.BETWEEN:
                    rawValue1 = rawValue1.Replace(">=", "");
                    rawValue1 = rawValue1.Replace("<=", "");
                    return rawValue1;
                case SearchOperator.BLANK:
                    return "";
                default:
                    return rawValue1;
            }
        }
    }
}
