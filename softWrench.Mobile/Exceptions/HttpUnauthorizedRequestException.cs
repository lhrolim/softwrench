using System;
using System.Net.Http;

namespace softWrench.Mobile.Exceptions
{
    [Serializable]
    public class HttpUnauthorizedRequestException : HttpRequestException
    {
        public HttpUnauthorizedRequestException()
            : base(null, null)
        {
        }

        public HttpUnauthorizedRequestException(string message)
            : base(message, null)
        {
        }

        public HttpUnauthorizedRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
