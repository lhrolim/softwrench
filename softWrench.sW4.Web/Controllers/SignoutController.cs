using System.Web.Mvc;
using System.Web.Security;
using cts.commons.Util;
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
                return Redirect("~/SignIn?ReturnUrl=%2f{0}%2f".Fmt(Request.ApplicationPath.Replace("/", "")));
            } catch {
                FormsAuthentication.SignOut();
                return Redirect("~/SignIn?ReturnUrl=%2f{0}%2f".Fmt(Request.ApplicationPath.Replace("/", "")));
            }

        }

    }
}
