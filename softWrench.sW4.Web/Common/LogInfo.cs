using System;
using System.Net;

namespace softWrench.sW4.Web.Common {
    public class LogInfo {
        private readonly string _userName;
        private readonly DateTime _beginDatetime;
        private DateTime _endDatetime;
        private readonly Uri _requestUri;
        private readonly string _methodName;

        public LogInfo(string userName, DateTime beginDatetime, Uri requestUri, String methodName) {
            _userName = userName;
            _beginDatetime = beginDatetime;
            _requestUri = requestUri;
            _methodName = methodName;
        }

        public string UserName {
            get { return _userName; }
        }

        public DateTime BeginDatetime {
            get { return _beginDatetime; }
        }

        public Uri RequestUri {
            get { return _requestUri; }
        }

        public String MethodName {
            get { return _methodName; }
        }

        public DateTime EndDateTime {
            get { return _endDatetime; }
            set { _endDatetime = value; }
        }

        public HttpStatusCode HttpStatusCode { get; set; }

        public override string ToString() {
            return
                string.Format(
                    "User Name: {0}, Time: {1} (ms), Request Uri: {2}, Method Name: {3}, Http Status Code: {4}",
                    UserName, Convert.ToInt32((EndDateTime - BeginDatetime).TotalMilliseconds), RequestUri, MethodName, HttpStatusCode);
        }
    }
}
