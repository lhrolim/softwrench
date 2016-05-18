using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace cts.commons.web.Util {
    public static class RestUtil {
        public static async Task<WebResponse> CallRestApi(string url, string method, Dictionary<string, string> headers = null, string payload = null) {
            var request = DoBuildRequest(url, method, headers, payload);
            // fetch response
            return await request.GetResponseAsync();
        }

        public static WebResponse CallRestApiSync(string url, string method, Dictionary<string, string> headers = null, string payload = null) {
            var request = DoBuildRequest(url, method, headers, payload);
            // fetch response
            return request.GetResponse();
        }

        private static HttpWebRequest DoBuildRequest(string url, string method, Dictionary<string, string> headers, string payload) {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.ContentType = "application/xml";
            // headers
            if (headers != null) {
                foreach (var header in headers) {
                    if (header.Key.Equals("Content-Type")) {
                        request.ContentType = header.Value;
                    } else {
                        request.Headers[header.Key] = header.Value;
                    }
                }
            }

            // write payload to requests stream
            if (!string.IsNullOrEmpty(payload)) {
                var body = Encoding.UTF8.GetBytes(payload);
                request.ContentLength = body.Length;
                using (var requestStream = request.GetRequestStream()) {
                    requestStream.Write(body, 0, body.Length);
                }
            }
            return request;
        }
    }
}