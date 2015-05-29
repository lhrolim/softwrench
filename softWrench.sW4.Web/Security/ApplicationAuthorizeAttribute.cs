using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using softWrench.sW4.Web.Common;
using AuthorizeAttribute = System.Web.Http.AuthorizeAttribute;

namespace softWrench.sW4.Web.Security {
    public class ApplicationAuthorizeAttribute : AuthorizeAttribute {
        protected override void HandleUnauthorizedRequest(HttpActionContext filterContext) {
            var request = filterContext.Request;

            if (System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated == false) {
                throw new UnauthorizedAccessException();
            }


            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
