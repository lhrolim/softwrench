using System.Web.Security;
using cts.commons.portable.Util;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.Models.Home;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using softWrench.sW4.Web.Security;

namespace softWrench.sW4.Web.Controllers {
    [System.Web.Mvc.Authorize]
    public class HomeController : Controller {

        //        private readonly IAPIControllerFactory _controllerFactory;

        private readonly IConfigurationFacade _facade;
        private readonly I18NResolver _i18NResolver;
        private readonly StatusColorResolver _statusColorResolver;
        private readonly ClassificationColorResolver _classificationColorResolver;
        private readonly ContextLookuper _lookuper;
        private MenuHelper.MenuHelper _menuHelper;






        public HomeController(IConfigurationFacade facade, I18NResolver i18NResolver, StatusColorResolver statusColorResolver, ContextLookuper lookuper, MenuHelper.MenuHelper menuHelper, ClassificationColorResolver classificationColorResolver) {
            //            _controllerFactory = (IAPIControllerFactory)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IAPIControllerFactory));
            _facade = facade;
            _i18NResolver = i18NResolver;
            _statusColorResolver = statusColorResolver;
            _lookuper = lookuper;
            _menuHelper = menuHelper;
            _classificationColorResolver = classificationColorResolver;
        }

        public ActionResult Index() {


            var user = SecurityFacade.CurrentUser();
            //TODO: allow mobile
            var menuModel = _menuHelper.BuildMenu(ClientPlatform.Web);
            var indexItemId = menuModel.Menu.ItemindexId;
            var indexItem = menuModel.Menu.ExplodedLeafs.FirstOrDefault(l => indexItemId.EqualsIc(l.Id));
            if (indexItem == null) {
                //first we´ll try to get the item declared, if it´s null (that item is role protected for that user, for instance, let´s pick the first leaf one as a fallback to avoid problems
                indexItem = menuModel.Menu.ExplodedLeafs.FirstOrDefault(a => a.Leaf);
            }
            //if still null logout the user, or an external link that would cause a redirect loop
            if (indexItem == null || (indexItem is ExternalLinkMenuItemDefinition)) {
                SignoutController.DoLogout(Session, Response);
                return Redirect("~/SignIn?forbidden=true&ReturnUrl=%2f{0}%2f".Fmt(Request.ApplicationPath.Replace("/", "")));
            }

            HomeModel model = null;
            string url;
            string title = "softWrench";
            if (indexItem is ApplicationMenuItemDefinition) {
                var app = (ApplicationMenuItemDefinition)indexItem;
                var key = new ApplicationMetadataSchemaKey(app.Schema, app.Mode, ClientPlatform.Web);
                var adapter = new DataRequestAdapter(null, key);
                url = GetUrlFromApplication(app.Application, adapter);
                //title = app.Title;
            } else if (indexItem is ActionMenuItemDefinition) {
                var actItem = (ActionMenuItemDefinition)indexItem;
                url = _menuHelper.GetUrlFromAction(actItem);
                //title = actItem.Title;
            } else {
                FormsAuthentication.SignOut();
                return Redirect("~/SignIn?ReturnUrl=%2f{0}%2f&forbidden=true".Fmt(Request.ApplicationPath.Replace("/", "")));
            }
            model = new HomeModel(url, title, FetchConfigs(), menuModel, user, HasPopupLogo(), _i18NResolver.FetchCatalogs(), _statusColorResolver.FetchCatalogs(), _classificationColorResolver.FetchCatalogs(), ApplicationConfiguration.ClientName);
            return View(model);
        }



        private HomeConfigs FetchConfigs() {
            var logoIcon = _facade.Lookup<string>(ConfigurationConstants.MainIconKey);
            var myProfileEnabled = _facade.Lookup<Boolean>(ConfigurationConstants.MyProfileEnabled);
            var clientSideLogLevel = _facade.Lookup<string>(ConfigurationConstants.ClientSideLogLevel);
            var invbalancesListScanOrder = _facade.Lookup<string>(ConfigurationConstants.InvbalancesListScanOrder);
            var newInvIssueDetailScanOrder = _facade.Lookup<string>(ConfigurationConstants.NewInvIssueDetailScanOrder);
            var invIssueListScanOrder = _facade.Lookup<string>(ConfigurationConstants.InvIssueListScanOrder);
            var physicalcountListScanOrder = _facade.Lookup<string>(ConfigurationConstants.PhysicalcountListScanOrder);

            var physicaldeviationListScanOrder = _facade.Lookup<string>(ConfigurationConstants.PhysicaldeviationListScanOrder);
            var reservedMaterialsListScanOrder = _facade.Lookup<string>(ConfigurationConstants.ReservedMaterialsListScanOrder);
            var matrectransTransfersListScanOrder = _facade.Lookup<string>(ConfigurationConstants.MatrectransTransfersListScanOrder);
            var invIssueListBeringScanOrder = _facade.Lookup<string>(ConfigurationConstants.InvIssueListBeringScanOrder);
            var newKeyIssueDetailScanOrder = _facade.Lookup<string>(ConfigurationConstants.NewKeyIssueDetailScanOrder);

            return new HomeConfigs() {
                Logo = logoIcon,
                MyProfileEnabled = myProfileEnabled,
                I18NRequired = MetadataProvider.GlobalProperties.I18NRequired(),
                ClientName = ApplicationConfiguration.ClientName,
                Environment = ApplicationConfiguration.Profile,
                IsLocal = ApplicationConfiguration.IsLocal(),
                ActivityStreamFlag = ApplicationConfiguration.ActivityStreamFlag,
                ClientSideLogLevel = clientSideLogLevel,
                SuccessMessageTimeOut = GetSuccessMessageTimeOut(),
                InitTimeMillis = ApplicationConfiguration.GetStartTimeInMillis(),
                InvbalancesListScanOrder = invbalancesListScanOrder,
                NewInvIssueDetailScanOrder = newInvIssueDetailScanOrder,
                InvIssueListScanOrder = invIssueListScanOrder,
                PhysicalcountListScanOrder = physicalcountListScanOrder,
                PhysicaldeviationListScanOrder = physicaldeviationListScanOrder,
                ReservedMaterialsListScanOrder = reservedMaterialsListScanOrder,
                MatrectransTransfersListScanOrder = matrectransTransfersListScanOrder,
                InvIssueListBeringScanOrder = invIssueListBeringScanOrder,
                DefaultEmail = MetadataProvider.GlobalProperty("defaultEmail"),
                UIShowClassicAdminMenu = ApplicationConfiguration.UIShowClassicAdminMenu
            };
        }

        private static int GetSuccessMessageTimeOut() {
            int timeout;
            int.TryParse(MetadataProvider.GlobalProperties.GlobalProperty(ApplicationSchemaPropertiesCatalog.SuccessMessageTimeOut), out timeout);
            if (timeout == 0) {
                timeout = 5000;
            }
            return timeout;
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
            var hasPopupLogo = HasPopupLogo(application, popupmode);
            var menuModel = _menuHelper.BuildMenu(ClientPlatform.Web);
            return View("Index", new HomeModel(redirectURL, null, FetchConfigs(), menuModel, user, hasPopupLogo, _i18NResolver.FetchCatalogs(), _statusColorResolver.FetchCatalogs(), _classificationColorResolver.FetchCatalogs(),
                ApplicationConfiguration.ClientName, windowTitle, message));
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


        private static bool HasPopupLogo(string application = null, string popupmode = null) {
            return ApplicationConfiguration.ClientName == "hapag" && popupmode == "browser";
        }

        private string GetUrlFromApplication(string application, DataRequestAdapter adapter) {
            var actionURL = String.Format("api/data/{0}", application);
            //TODO: fix WEBAPIUTIL method
            var queryString = "key[schemaId]=" + adapter.Key.SchemaId + "&key[mode]=" +
                                 adapter.Key.Mode.ToString().ToLower() + "&key[platform]=" +
                                 adapter.Key.Platform.ToString().ToLower();
            return WebAPIUtil.GetRelativeRedirectURL(actionURL, queryString);
        }







    }
}
