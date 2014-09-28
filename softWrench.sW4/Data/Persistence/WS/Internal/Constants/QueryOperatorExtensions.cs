using System;

namespace softWrench.sW4.Data.Persistence.WS.Internal.Constants
{
    static class QueryOperatorExtensions
    {
        public static String MaximoValue(this QueryOperator queryOperator)
        {
            switch (queryOperator) {
                case QueryOperator.Equals:
                    return "Item";
                case QueryOperator.Diff:
                    return "Item1";
                case QueryOperator.Lt:
                    return "Item2";
                case QueryOperator.Lte:
                    return "Item3";
                case QueryOperator.Gt:
                    return "Item4";
                case QueryOperator.Gte:
                    return "Item5";
                default:
                    return "Item";
            }
        }
    }
}
