using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using cts.commons.portable.Util;
using log4net;

namespace cts.commons.Util {
    public class LoggingUtil {


        public static ILog DefaultLog = LogManager.GetLogger("DEFAULT_LOG");

        public static string MsDelta(Stopwatch watch) {
            watch.Stop();
            return watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) + " ms";
        }

        public static Stopwatch StartMeasuring(ILog log, string msg, params object[] parameters) {
            log.DebugFormat(msg, parameters);
            return Stopwatch.StartNew();
        }

        public static string BaseDurationMessage(string msg, Stopwatch before) {
            return String.Format(msg, MsDelta(before));
        }

        public static string BaseDurationMessageFormat(Stopwatch before, string msg, params object[] parameters) {
            return String.Format(msg, parameters) + "| Time ellapsed: " + MsDelta(before);
        }

        public static string ReplaceParameters(string queryst, params object[] parameters) {
            if (parameters == null || !parameters.Any()) {
                return queryst + " ";
            }
            foreach (var parameter in parameters) {
                if (parameter is ExpandoObject) {
                    var ob = (ExpandoObject)parameter;
                    foreach (var item in ob) {
                        if (item.Value is ICollection) {
                            var list = new List<string>();
                            foreach (var collItem in (IEnumerable)item.Value) {
                                list.Add("'" + Convert.ToString(collItem) + "'");
                            }
                            var result = string.Join(",", list);
                            //                            sb.Append(item.Key + "=" + result + ";");
                            queryst = queryst.Replace(":" + item.Key, result);
                        } else {
                            if (item.Value is string) {
                                queryst = queryst.Replace(":" + item.Key, "'" + item.Value + "'");
                            }
                        }
                    }
                } else {
                    queryst = queryst.ReplaceFirstOccurrence("?", "'" + parameter + "'");
                }
            }
            return queryst;
        }

        public static string QueryStringForLogging(string queryst, string queryAlias, params object[] parameters) {
            queryst = ReplaceParameters(queryst, parameters);
            return queryAlias == null ? queryst : queryAlias + ": " + queryst;
        }


    }
}
