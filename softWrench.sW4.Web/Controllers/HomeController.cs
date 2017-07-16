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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using softWrench.sW4.AUTH;
using softWrench.sW4.Web.Security;

namespace softWrench.sW4.Web.Controllers {
    [System.Web.Mvc.Authorize]
    public class HomeController : Controller {

        //        private readonly IAPIControllerFactory _controllerFactory;

        private readonly IConfigurationFacade _facade;
        private readonly I18NResolver _i18NResolver;
        private ContextLookuper _lookuper;

        public HomeController(IConfigurationFacade facade, I18NResolver i18NResolver, ContextLookuper lookuper) {
            //            _controllerFactory = (IAPIControllerFactory)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IAPIControllerFactory));
            _facade = facade;
            _i18NResolver = i18NResolver;
            _lookuper = lookuper;
        }

        public ActionResult Index() {
            var user = SecurityFacade.CurrentUser();
            var securedMenu = user.Menu(ClientPlatform.Web);
            var indexItemId = securedMenu.ItemindexId;
            var indexItem = securedMenu.Leafs.FirstOrDefault(l => indexItemId.EqualsIc(l.Id));
            if (indexItem == null) {
                //first we´ll try to get the item declared, if it´s null (that item is role protected for that user, for instance, let´s pick the first leaf one as a fallback to avoid problems
                indexItem = securedMenu.Leafs.FirstOrDefault(a => a.Leaf);
            }

            HomeModel model = null;
            if (indexItem is ApplicationMenuItemDefinition) {
                var app = (ApplicationMenuItemDefinition)indexItem;
                var key = new ApplicationMetadataSchemaKey(app.Schema, app.Mode, ClientPlatform.Web);
                var adapter = new DataRequestAdapter(null, key);
                model = new HomeModel(GetUrlFromApplication(app.Application, adapter), app.Title, FetchConfigs(), user, HasPopupLogo(), _i18NResolver.FetchCatalogs(), ApplicationConfiguration.ClientName, indexItem.Module);
            } else if (indexItem is ActionMenuItemDefinition) {
                var actItem = (ActionMenuItemDefinition)indexItem;
                var action = actItem.Action;
                model = new HomeModel(GetUrlFromAction(actItem), actItem.Title, FetchConfigs(), user, HasPopupLogo(), _i18NResolver.FetchCatalogs(), ApplicationConfiguration.ClientName, indexItem.Module);
            }
            return View(model);
        }

        private HomeConfigs FetchConfigs() {
            var logoIcon = _facade.Lookup<string>(ConfigurationConstants.MainIconKey);
            var myProfileEnabled = _facade.Lookup<Boolean>(ConfigurationConstants.MyProfileEnabled);
            var clientSideLogLevel = _facade.Lookup<string>(ConfigurationConstants.ClientSideLogLevel);
            return new HomeConfigs() {
                Logo = logoIcon,
                AllowedFileTypes = ApplicationConfiguration.AllowedFilesExtensions,
                MyProfileEnabled = myProfileEnabled,
                I18NRequired = MetadataProvider.GlobalProperties.I18NRequired(),
                ClientName = ApplicationConfiguration.ClientName,
                Environment = ApplicationConfiguration.Profile,
                IsLocal = ApplicationConfiguration.IsLocal(),
                ClientSideLogLevel = clientSideLogLevel,
                SuccessMessageTimeOut = GetSuccessMessageTimeOut(),
                InitTimeMillis = ApplicationConfiguration.SystemBuildDateInMillis
            };
        }

        private static int GetSuccessMessageTimeOut() {
            int timeout;
            int.TryParse(MetadataProvider.GlobalProperties.GlobalProperty(ApplicationSchemaPropertiesCatalog.SuccessMessageTimeOut), out timeout);
            if (timeout == 0) {
                timeout = 10000;
            }
            return timeout;
        }


        public new ActionResult RedirectToHash(string hash) {
            return null;
        }



        public ActionResult RedirectToAction(string application, string controllerToRedirect, string popupmode, string actionToRedirect, string queryString, string message, string messageType) {
            string actionURL;
            var user = SecurityFacade.CurrentUser();
            if (application != null) {
                actionURL = String.Format("api/data/crud/{0}/", application);
            } else {
                //TODO: actions parameters missing...
                actionURL = String.Format("api/generic/{0}/{1}", controllerToRedirect, actionToRedirect);
            }
            var unescapedQs = WebAPIUtil.GetUnescapedQs(queryString);
            var allowed = application== "solution" || ValidateSecurity(unescapedQs);
        
            var redirectURL = String.Format("{0}?{1}", actionURL, unescapedQs);

            var windowTitle = GetWindowTitle(redirectURL);
            var hasPopupLogo = HasPopupLogo(application, popupmode);

            var homeModel = new HomeModel(redirectURL, null, FetchConfigs(), user, hasPopupLogo,
                _i18NResolver.FetchCatalogs(), ApplicationConfiguration.ClientName, _lookuper.LookupContext().Module, windowTitle, message, messageType);
            homeModel.Allowed = allowed;

            return View("Index", homeModel);
        }

        private static bool ValidateSecurity(string unescapedQs) {
            var qsValues = HttpUtility.ParseQueryString(unescapedQs);
            var ids = qsValues.GetValues("id");
            var hashs = qsValues.GetValues("hmachash");
            if (ids == null) {
                return true;
            }

            if (ids != null && hashs == null) {
                return false;
            }
            var hash = hashs[0];
            if (!AuthUtils.HmacShaEncode(ids[0]).Equals(hash)) {
                return false;
            }
            return true;
        }

        private string GenerateHashString() {

            var nameValues = HttpUtility.ParseQueryString(Request.QueryString.ToString());
            nameValues.Set("sortBy", "4");
            string url = Request.Url.AbsolutePath;
            Response.Redirect(url + "?" + nameValues); // ToString() is called implicitly
            return null;
        }

        public ActionResult MakeSWAdmin() {
            return RedirectToAction(null, "MakeSWAdmin", null, "Index", null, null, null);
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

        private string GetUrlFromAction(ActionMenuItemDefinition item) {
            var action = item.Action;
            if (String.IsNullOrWhiteSpace(action)) {
                action = "Get";
            }
            string controller = item.Controller;
            var queryString = String.Join("&", item.Parameters.ToArray());
            var actionURL = String.Format("api/generic/{0}/{1}", controller, action);
            return WebAPIUtil.GetRelativeRedirectURL(actionURL, queryString);
        }





    }
}
