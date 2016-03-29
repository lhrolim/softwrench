using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace cts.commons.web.Util {
    public static class RestUtil {
        public static async Task<WebResponse> CallRestApi(string url, string method, Dictionary<string, string> headers = null, string payload = null) {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            // headers
            if (headers != null) {
                foreach (var header in headers) {
                    request.Headers[header.Key] = header.Value;
                }

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
            return await request.GetResponseAsync();
        }
    }
}