using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.EL.ToString
{
    public class ToStringELParser
    {
        public static ToStringExpression ParseExpression(String expression)
        {
            int startIndex = 0;
            IList<Tuple<int, int>> varPositions = new List<Tuple<int, int>>();
            IList<string> varNames = new List<string>();

            int lastPosition = expression.Length - 1;
            do
            {
                var @idx = expression.IndexOf("@", startIndex, System.StringComparison.Ordinal);
                if (@idx == -1)
                {
                    break;
                }
                var wsIdx = expression.IndexOf(" ", idx, System.StringComparison.Ordinal);
                if (wsIdx == -1)
                {
                    //@ found at the end of the string
                    varNames.Add(expression.MySubString(@idx+1, lastPosition));
                    startIndex = lastPosition;
                }
                else
                {
                    varNames.Add(expression.MySubString(@idx+1, wsIdx-1));
                    startIndex = wsIdx;
                }
                
            } while (expression.IndexOf("@", startIndex, System.StringComparison.Ordinal) != -1);

            
            for (var i = 0; i < varNames.Count; i++)
            {
                string varName = varNames[i];
                expression = expression.Replace("@"+varName, "{" + i + "}");
            }

            return new ToStringExpression(expression,varNames);

        }
    }

    public static partial class MyExtensions
    {
        public static string MySubString(this string s, int start, int end)
        {
            return s.Substring(start, end - start + 1);
        }
    }
}
