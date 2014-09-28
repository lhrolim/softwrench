using System;
using System.Net.Http;

namespace softWrench.Mobile.Exceptions
{
    [Serializable]
    public class ExtendedHttpRequestException : HttpRequestException
    {
        private readonly string _response;

        public ExtendedHttpRequestException(string response)
            : base(null, null)
        {
            _response = response;
        }

        public ExtendedHttpRequestException(string response, string message)
            : base(message, null)
        {
            _response = response;
        }

        public ExtendedHttpRequestException(string response, string message, Exception inner)
            : base(message, inner)
        {
            _response = response;
        }

        public string Response
        {
            get { return _response; }
        }
    }
}