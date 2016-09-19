using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Util;
using System.Web;
using System.ServiceModel.Channels;

namespace softWrench.sW4.Web.Util {
    public static class RequestUtil {

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

        /// <summary>
        /// Extension method for the <see cref="HttpRequestMessage"/> class.
        /// Gets the IP address from the <see cref="HttpRequestMessage"/> object.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> object.</param>
        /// <returns>The IP address as string if its available else returns null</returns>
        public static string GetIPAddress(this HttpRequestMessage request) {
            if (request.Properties.ContainsKey("MS_HttpContext")) {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }

            if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name)) {
                RemoteEndpointMessageProperty prop;
                prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }

            return null;
        }
    }
}