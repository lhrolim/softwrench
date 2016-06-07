using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;

namespace softWrench.sW4.Web.Util {
    public class RequestUtil {

        public static void ValidateMockError(HttpRequestMessage request) {
            if (request == null) {
                return;
            }

            var mockError = RequestUtil.GetValue(request, "mockerror");
            if (!"true".Equals(mockError) || !ApplicationConfiguration.IsLocal()) {
                return;
            }
            try {
                //forcing a generic exception...
                object a = new { };
                var b = (string)a;
            } catch (Exception e) {
                var errorResponse = request.CreateResponse(HttpStatusCode.InternalServerError,
                    new ErrorDto(e.Message, e.StackTrace, e.StackTrace));
                throw new HttpResponseException(errorResponse);
            }
        }


        public static string GetValue(HttpRequestMessage request, string key) {
            IEnumerable<string> headers;
            string value;            
            if (request.Headers.TryGetValues(key, out headers)) {
                value = headers.First();
            } else {
                var pair = request.GetQueryNameValuePairs().FirstOrDefault(f => f.Key == key);
                value = pair.Value;
            }
            return "null".Equals(value) ? null : value;
        }

    }
}