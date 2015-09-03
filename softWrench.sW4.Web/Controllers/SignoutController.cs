using System;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using cts.commons.portable.Util;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Controllers {
    public class SignoutController : Controller {

        private readonly IEventDispatcher _eventDispatcher;

        public SignoutController(IEventDispatcher eventDispatcher) {
            _eventDispatcher = eventDispatcher;
        }

        //
        // GET: /Signout/
        public ActionResult SignOut() {
            try {
                var user = SecurityFacade.CurrentUser();
                if (ApplicationConfiguration.IsDev() || user.IsSwAdmin()) {
                    MetadataProvider.StubReset();
                    _eventDispatcher.Dispatch(new ClearCacheEvent());
                }
                FormsAuthentication.SignOut();

                Session.Clear();  // This may not be needed -- but can't hurt
                Session.Abandon();

                // Clear authentication cookie
                HttpCookie rFormsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
                rFormsCookie.Path = HostingEnvironment.ApplicationVirtualPath;
                rFormsCookie.Expires = DateTime.Now.AddYears(-1);
                Response.Cookies.Add(rFormsCookie);

                // Clear session cookie 
                var rSessionCookie = new HttpCookie("ASP.NET_SessionId", "");
                rSessionCookie.Expires = DateTime.Now.AddYears(-1);
                rSessionCookie.Path = HostingEnvironment.ApplicationVirtualPath;
                Response.Cookies.Add(rSessionCookie);


                return Redirect("~/SignIn?ReturnUrl=%2f{0}%2f".Fmt(Request.ApplicationPath.Replace("/", "")));
            } catch {
                FormsAuthentication.SignOut();
                return Redirect("~/SignIn?ReturnUrl=%2f{0}%2f".Fmt(Request.ApplicationPath.Replace("/", "")));
            }

        }

    }
}
