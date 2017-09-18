using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softwrench.sw4.api.classes.exception;

namespace softwrench.sw4.offlineserver.model.exception {

    public class InvalidOffLineVersionException : InvalidStateException, IOfflineSyncException {

        public const string Msg = "The version {0} is not supported anymore. Please updated to the latest version or contact support";

        public InvalidOffLineVersionException(string currentVersion) : base(Msg.Fmt(currentVersion)) {

        }

        public HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
        public bool NotifyException => true;
        public bool RequestSupportReport => false;
    }
}
