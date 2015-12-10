using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NHibernate.Linq;

namespace softWrench.sW4.Web.Util {
    public static class RestUtil {
        public static async Task<HttpWebResponse> CallRestApi(string url, string method, Dictionary<string, string> headers = null, string payload = null) {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            // headers
            if (headers != null) {
                headers.ForEach((e) => request.Headers[e.Key] = e.Value);
            }
            request.ContentType = "application/xml";
            // write payload to requests stream
            if (!string.IsNullOrEmpty(payload)) {
                var body = Encoding.UTF8.GetBytes(payload);
                request.ContentLength = body.Length;
                using (var requestStream = request.GetRequestStream()) {
                    requestStream.Write(body, 0, body.Length);
                }
            }
            // fetch response
            return (HttpWebResponse)request.GetResponse();
        }
    }
}