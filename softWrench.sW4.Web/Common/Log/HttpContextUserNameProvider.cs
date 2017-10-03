using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace softWrench.sW4.Web.Common.Log {
    public class HttpContextUserNameProvider {

        public override string ToString() {
            var context = HttpContext.Current;
            if (context != null && context.User != null && context.User.Identity.IsAuthenticated) {
                return context.User.Identity.Name;
            }
            return "";
        }

    }
}