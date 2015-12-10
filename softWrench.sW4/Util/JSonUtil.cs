using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;

namespace softWrench.sW4.Util {
    public static class JSonUtil {

        private static readonly ILog Log = LogManager.GetLogger(typeof(JSonUtil));


        public static void ReplaceValue(this JObject ob, string propertyName, JToken value) {
            ob.Remove(propertyName);
            ob.Add(propertyName, value);

        }


        public static String StringValue(this JObject ob, string propertyName) {
            var prop = ob.Property(propertyName);
            if (prop == null || prop.Value == null) {
                return null;
            }
            return prop.Value.ToString();
        }


        public static JObject BuildJSon(string filePath) {
            var sb = new StringBuilder();
            FileUtils.DoWithLines(filePath, s => NormalizeJson(s, sb));
            var json = "{" + sb + "}";
            Log.Debug("Parsing labels JSON: \n " + json);
            return JObject.Parse(json);
        }

        public static void NormalizeJson(string s, StringBuilder sb) {
            if (String.IsNullOrEmpty(s)) {
                return;
            }

            if ((s.Contains("{") || s.Contains("}")) && !(s.Contains("{") && s.Contains("}"))) {
                //both at same line could indicate parameters {0}
                if (!s.Contains(",") && s.Contains("}")) {
                    //append , to indicate the end of an object
                    sb.AppendLine(s + ",");
                } else {
                    var idx = s.IndexOf(':');
                    if (idx != -1) {
                        var splitObj = new string[2] { s.Substring(0, idx), s.Substring(idx + 1, s.Length - idx - 1) };
                        s = s.Replace(splitObj[0], "'" + splitObj[0].Trim() + "'");
                    }
                    sb.AppendLine(s);
                }
                return;
            }
            var keySeparatorIdx = s.IndexOf(':');
            if (keySeparatorIdx == -1) {
                //no :, could be } or {
                sb.AppendLine(s);
                return;
            }

            var split = new string[2] { s.Substring(0, keySeparatorIdx), s.Substring(keySeparatorIdx + 1, s.Length - keySeparatorIdx - 1) };

            if (split.Count() != 2) {
                sb.AppendLine(s);
                return;
            }
            var key = split[0].Trim();
            key = "'" + key + "'";
            var value = split[1].Trim();
            if (!value.EndsWith(",")) {
                if ((value.StartsWith("'") && value.EndsWith("'")) || (value.StartsWith("\"") && value.EndsWith("\""))) {
                    //already set the ' or " making it a default json, no need to append again
                    value = value + ",";
                } else {
                    value = "'" + value + "',";
                }
            } else {
                value = "'" + value.Substring(0, value.Length - 1) + "',";
            }
            var result = key + ":" + value;
            sb.AppendLine(result);
        }
    }
}
