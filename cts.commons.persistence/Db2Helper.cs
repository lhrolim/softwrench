using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate.Util;

namespace cts.commons.persistence {
    public class Db2Helper {

        private const string NamedParameterPattern = "\\:\\w+";
        private static readonly Regex NamedParameterRegex = new Regex(NamedParameterPattern);

        /// <summary>
        /// Fix db2 limitation of using the same named parameter multiple times.
        /// </summary>
        public static string FixNamedParameters(string queryst, ExpandoObject parameters) {
            var innerQueryst = queryst;


            var collectionMatches = NamedParameterRegex.Matches(queryst);

            // if no or just one named parameters are found there is no chance of 
            // a named parameter appears more than once, just returns
            if (collectionMatches.Count < 2) {
                return queryst;
            }

            var parametersMap = new Dictionary<string, object>();
            var parameterCount = new Dictionary<string, int>();
            var hasParameterRepetition = false;

            // save the count of each named parameter appearance
            foreach (Match match in collectionMatches) {
                var parameter = match.Captures[0].Value;
                if (parameterCount.ContainsKey(parameter)) {
                    parameterCount[parameter] = parameterCount[parameter] + 1;
                    hasParameterRepetition = true;
                } else {
                    parameterCount.Add(parameter, 1);
                }
            }

            // if there is no repetition of named parameters just returns
            if (!hasParameterRepetition) {
                return queryst;
            }

            // changes the query from end to beginning
            // each multiple parameter gets a number of repetition after its names on query
            // and a new entry is added on parameters values
            for (var i = collectionMatches.Count - 1; i >= 0; i--) {
                var capture = collectionMatches[i].Captures[0];
                var parameter = capture.Value;
                var count = parameterCount[parameter];

                if (count == 1) {
                    continue;
                }

                parameterCount[parameter] = parameterCount[parameter] - 1;
                var parameterName = parameter.Substring(1);
                var newParameterName = parameterName + count;
                var pair = parameters.First(p => parameterName.Equals(p.Key));
                parametersMap.Add(newParameterName, pair.Value);

                var index = capture.Index;
                var qryBegin = innerQueryst.Substring(0, index);
                var qryEnd = innerQueryst.Substring(index + parameter.Length);
                innerQueryst = qryBegin + ":" + newParameterName + qryEnd;
            }

            parameters.AddOrOverride(parametersMap);
            return innerQueryst;
        }
    }
}
