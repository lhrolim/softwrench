using System.Web.Mvc;
using System.Web.Security;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Controllers {
    public class SignoutController : Controller {
        //
        // GET: /Signout/
        public ActionResult SignOut() {
            if (ApplicationConfiguration.IsDev()) {
                MetadataProvider.StubReset();
            }
            FormsAuthentication.SignOut();
            return Redirect("~/SignIn?ReturnUrl=%2f{0}%2f".Fmt(Request.ApplicationPath.Replace("/","")));
        }

        public ActionResult SignOutClosePage() {
            if (ApplicationConfiguration.IsDev()) {
                MetadataProvider.StubReset();
            }
            FormsAuthentication.SignOut();
            return Redirect("~/SignIn?ReturnUrl=%2f{0}%2f".Fmt(Request.ApplicationPath.Replace("/", "")));
        }

    }
}
