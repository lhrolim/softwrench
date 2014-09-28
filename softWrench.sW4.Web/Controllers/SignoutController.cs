using System.Web.Mvc;
using System.Web.Security;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector.Events;
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
            var user = SecurityFacade.CurrentUser();
            if (ApplicationConfiguration.IsDev() || user.IsSwAdmin()) {
                MetadataProvider.StubReset();
                _eventDispatcher.Dispatch(new ClearCacheEvent());
            }
            FormsAuthentication.SignOut();
            return Redirect("~");
        }

    }
}
