using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DocumentFormat.OpenXml.Spreadsheet;

namespace softWrench.sW4.Web.Common {
    public class WebAPIUtil {


        public const string EqualSeparator = "$";
        public const string AndSeparator = "@";



        public static string RemoveControllerSufix(Type registration) {
            return registration.Name.Replace("Controller", "");
        }

        public static string RemoveControllerSufix(string name) {
            return name.Replace("Controller", "");
        }

        public static string GetQueryString(object obj) {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + p.GetValue(obj, null);

            return String.Join("&", properties.ToArray());
        }

        public static string GetFullRedirectURL(string protocol, string host, int port, string actionURL, string queryString) {
            return String.Format("{0}://{1}:{2}/{3}?{4}", protocol, host, port, actionURL, queryString);
        }

        public static string GetRelativeRedirectURL(string actionURL, string queryString) {
            if (queryString != null) {
                queryString = queryString.Replace(AndSeparator, "&");
                queryString = queryString.Replace(EqualSeparator, "=");
            }
            return String.Format("{0}?{1}", actionURL, queryString);
        }

        public static string GetUnescapedQs(string queryString) {
            if (queryString != null) {
                queryString = queryString.Replace(AndSeparator, "&");
                queryString = queryString.Replace(EqualSeparator, "=");
            }
            return queryString;
        }


        public static void AppendToQueryString(StringBuilder sb, string key, object value) {
            sb.Append(key + EqualSeparator + value + AndSeparator);
        }
    }
}