using System.Web.Security;
using cts.commons.portable.Util;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu;
using softWrench.sW4.Data.API;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.Models.Home;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softWrench.sW4.Web.Controllers {
    [System.Web.Mvc.Authorize]
    public class HomeController : Controller {

        private MenuHelper.MenuHelper _menuHelper;
        private readonly HomeService _homeService;

        public HomeController(MenuHelper.MenuHelper menuHelper, HomeService homeService) {
            _menuHelper = menuHelper;
            _homeService = homeService;
        }

        public async Task<ActionResult> Index(string application, string schemaid, string id) {
            var user = SecurityFacade.CurrentUser();
            //TODO: allow mobile
            var menuModel = _menuHelper.BuildMenu(ClientPlatform.Web);
            var indexItem = GetMenuIndex(menuModel);
            //if still null logout the user, or an external link that would cause a redirect loop
            if (indexItem == null || (indexItem is ExternalLinkMenuItemDefinition)) {
                SignoutController.DoLogout(Session, Response);
                return Redirect("~/SignIn?forbidden=true&ReturnUrl=%2f{0}%2f".Fmt(Request.ApplicationPath.Replace("/", "")));
            }

            string url;
            string title = "softWrench";
            if (indexItem is ApplicationMenuItemDefinition) {
                var app = (ApplicationMenuItemDefinition)indexItem;
                var key = new ApplicationMetadataSchemaKey(app.Schema, app.Mode, ClientPlatform.Web);
                var adapter = new DataRequestAdapter(null, key);
                url = _homeService.GetUrlFromApplication(app.Application, adapter);
                //title = app.Title;
            } else if (indexItem is ActionMenuItemDefinition) {
                var actItem = (ActionMenuItemDefinition)indexItem;
                url = _menuHelper.GetUrlFromAction(actItem);
                //title = actItem.Title;
            } else {
                FormsAuthentication.SignOut();
                return Redirect("~/SignIn?ReturnUrl=%2f{0}%2f&forbidden=true".Fmt(Request.ApplicationPath.Replace("/", "")));
            }

            if (await _homeService.VerifyChangePassword(user, Response)) {
                return null;
            }

            var model = _homeService.BaseHomeModel(Request, user, menuModel);
            model.Url = url;
            model.Title = title;

            return View(model);
        }

        private MenuBaseDefinition GetMenuIndex(MenuModel menuModel) {
            MenuBaseDefinition indexItem = null;
            var indexItemId = menuModel.Menu.ItemindexId;
            indexItem = menuModel.Menu.ExplodedLeafs.FirstOrDefault(l => indexItemId.EqualsIc(l.Id));
            if (indexItem == null) {
                //first we´ll try to get the item declared, if it´s null (that item is role protected for that user, for instance, let´s pick the first leaf one as a fallback to avoid problems
                indexItem = menuModel.Menu.ExplodedLeafs.FirstOrDefault(a => a.Leaf);
            }
            return indexItem;
        }

        public ActionResult RedirectToAction(string application, string controllerToRedirect, string popupmode, string actionToRedirect, string queryString, string message) {
            string actionURL;
            var user = SecurityFacade.CurrentUser();
            if (application != null) {
                actionURL = String.Format("api/data/crud/{0}/", application);
            } else {
                //TODO: actions parameters missing...
                actionURL = String.Format("api/generic/{0}/{1}", controllerToRedirect, actionToRedirect);
            }
            var redirectURL = WebAPIUtil.GetRelativeRedirectURL(actionURL, queryString);

            var windowTitle = GetWindowTitle(redirectURL);
            var hasPopupLogo = _homeService.HasPopupLogo(application, popupmode);

            var model = _homeService.BaseHomeModel(Request, user);
            model.Url = redirectURL;
            model.HasPopupLogo = hasPopupLogo;
            model.WindowTitle = windowTitle;
            model.Message = message;
            return View("Index", model);
        }

        public ActionResult MakeSWAdmin() {
            return RedirectToAction(null, "MakeSWAdmin", null, "Index", null, null);
        }

        private string GetWindowTitle(string redirectUrl) {
            string title = null;
            try {
                var r = Regex.Split(redirectUrl, "faqid=");
                if (r.Length > 1) {
                    var r2 = Regex.Split(r[1], "&");
                    var faqid = r2[0];
                    if (!string.IsNullOrEmpty(faqid)) {
                        title = faqid;
                    }
                }
                if (!string.IsNullOrEmpty(title)) {
                    ViewBag.Title = title;
                }
                return title;
            } catch (Exception) {
                return title;
            }
        }
    }
}
