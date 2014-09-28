using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace softWrench.sW4.Web.Common.Log {
    public class LogInfo {
        public long ID { get; set; }
        public string Url { get; set; }
        public string RequestType { get; set; }
        public string RequestHeader { get; set; }
        public string RequestBody { get; set; }
        public string IPAddress { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public DateTime RequestDate { get; set; }
        public string ResponseHeader { get; set; }
        public string ResponseBody { get; set; }
        public DateTime ResponseDate { get; set; }

        public string UserName { get; set; }

        public override string ToString() {
            return
                string.Format(
                    "User Name: {0}, Time: {1} (ms), Request Uri: {2}, Controller:{3}, Method Name: {4}, Http Status Code: {5}",
                    UserName, Convert.ToInt32((ResponseDate - RequestDate).TotalMilliseconds), Url, Controller,Action, ResponseHeader);
        }
    }
}