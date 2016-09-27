using System;
using System.Web;
using cts.commons.simpleinjector;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.Controllers.MenuHelper;

namespace softWrench.sW4.Web.Models.Home {
    public class HomeService : ISingletonComponent {
        private readonly IConfigurationFacade _facade;
        private readonly UserManager _userManager;
        private readonly I18NResolver _i18NResolver;
        private readonly StatusColorResolver _statusColorResolver;
        private readonly ClassificationColorResolver _classificationColorResolver;
        private readonly MenuHelper _menuHelper;
        private readonly RouteService _routeService;

        public HomeService(IConfigurationFacade facade, UserManager userManager, I18NResolver i18NResolver, StatusColorResolver statusColorResolver, ClassificationColorResolver classificationColorResolver, MenuHelper menuHelper, RouteService routeService) {
            _facade = facade;
            _userManager = userManager;
            _i18NResolver = i18NResolver;
            _statusColorResolver = statusColorResolver;
            _classificationColorResolver = classificationColorResolver;
            _menuHelper = menuHelper;
            _routeService = routeService;
        }

        public virtual bool HasPopupLogo(string application = null, string popupmode = null) {
            return ApplicationConfiguration.ClientName == "hapag" && popupmode == "browser";
        }

        public virtual string GetUrlFromApplication(string application, DataRequestAdapter adapter) {
            return GetUrlFromApplication(application, adapter.Key.SchemaId, adapter.Key.Mode, adapter.Key.Platform);
        }

        public virtual string GetUrlFromApplication(string application, ApplicationSchemaDefinition schema, string id) {
            return GetUrlFromApplication(application, schema.SchemaId, schema.Mode, schema.Platform, id);
        }

        public virtual string GetUrlFromApplication(string application, ApplicationSchemaDefinition schema, string userid, string siteid) {
            return GetUrlFromApplication(application, schema.SchemaId, schema.Mode, schema.Platform, null, userid, siteid);
        }

        protected virtual string GetUrlFromApplication(string application, string schemaId, SchemaMode? mode, ClientPlatform? platform, string id = null, string userid = null, string siteid = null) {
            var actionURL = string.Format("api/data/{0}", application);
            //TODO: fix WEBAPIUTIL method
            var modeStr = mode.ToString().ToLower();
            var platformStr = platform.ToString().ToLower();
            var queryString = string.Format("key[schemaId]={0}&key[mode]={1}&key[platform]={2}", schemaId, modeStr, platformStr);
            if (!string.IsNullOrEmpty(id)) {
                queryString += "&id=" + id;
            }
            if (!string.IsNullOrEmpty(userid)) {
                queryString += "&userId=" + userid;
            }
            if (!string.IsNullOrEmpty(siteid)) {
                queryString += "&siteId=" + siteid;
            }
            return WebAPIUtil.GetRelativeRedirectURL(actionURL, queryString);
        }

        public virtual bool VerifyChangePassword(InMemoryUser user, HttpResponseBase response) {
            if (!_userManager.VerifyChangePassword(user)) {
                return false;
            }
            response.Redirect("~/UserSetup/ChangePassword");
            return true;
        }

        public virtual HomeModel BaseHomeModel(HttpRequestBase request, InMemoryUser user = null, MenuModel menuModel = null) {
            return new HomeModel(
                null,
                null,
                FetchConfigs(),
                menuModel ?? _menuHelper.BuildMenu(ClientPlatform.Web),
                user ?? SecurityFacade.CurrentUser(),
                HasPopupLogo(),
                _i18NResolver.FetchCatalogs(),
                _statusColorResolver.FetchCatalogs(),
                _statusColorResolver.FetchFallbackCatalogs(),
                _classificationColorResolver.FetchCatalogs(),
                ApplicationConfiguration.ClientName) {
                RouteInfo = _routeService.GetRouteInfo(request),
                ApplicationVersion = ApplicationConfiguration.SystemVersion
            };
        }

        protected virtual HomeConfigs FetchConfigs() {
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

            var SWdisplayableFormats = new SWdisplayableFormats() {
                DateTimeFormat = _facade.Lookup<string>(ConfigurationConstants.DateTimeFormat)
            };

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
                UIShowClassicAdminMenu = ApplicationConfiguration.UIShowClassicAdminMenu,
                UIShowToolbarLabels = ApplicationConfiguration.UIShowToolbarLabels,
                DisplayableFormats = SWdisplayableFormats
            };
        }

        protected virtual int GetSuccessMessageTimeOut() {
            int timeout;
            int.TryParse(MetadataProvider.GlobalProperties.GlobalProperty(ApplicationSchemaPropertiesCatalog.SuccessMessageTimeOut), out timeout);
            if (timeout == 0) {
                timeout = 5000;
            }
            return timeout;
        }
    }
}