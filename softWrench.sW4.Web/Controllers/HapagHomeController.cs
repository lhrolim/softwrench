using System.Linq;
using softwrench.sw4.Hapag.Data;
using softwrench.sw4.Hapag.Data.Init;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.Models.Hapag;
using System.Collections.Generic;
using System.Web.Http;
using softWrench.sW4.Web.SPF;
using c = softwrench.sw4.Hapag.Data.HapagDashBoardsConstants;

namespace softWrench.sW4.Web.Controllers {

    [System.Web.Mvc.Authorize]
    [SPFRedirect(URL = "Application", CrudSubTemplate = "/Templates/hapag/HapagHome.html")]
    [SWControllerConfiguration]
    public class HapagHomeController : ApiController {

        private readonly IConfigurationFacade _configFacade;
        private IWhereClauseFacade _whereClauseFacade;
        private readonly I18NResolver _i18Nresolver;

        private static IContextLookuper _contextManager;

        protected static IContextLookuper ContextManager {
            get {
                if (_contextManager != null) {
                    return _contextManager;
                }
                _contextManager =
                    SimpleInjectorGenericFactory.Instance.GetObject<IContextLookuper>(typeof(IContextLookuper));
                return _contextManager;
            }
        }

        private readonly IDictionary<string, DashboardDefinition> _dashboards = new Dictionary<string, DashboardDefinition>();

        public HapagHomeController(IConfigurationFacade configFacade, IWhereClauseFacade whereClauseFacade, I18NResolver i18Nresolver) {
            _configFacade = configFacade;
            _whereClauseFacade = whereClauseFacade;
            _i18Nresolver = i18Nresolver;
            BuildDict();
        }

        private void BuildDict() {
            var key = c.ActionRequiredForOpenRequests;
            _dashboards[key] = DashboardDefinition.Get5ElementsInstance(
            key, "servicerequest", "dashboardList",
            _i18Nresolver.I18NValue("dashboard.actionrequiredopenrequests", c.GetDefaultI18NValue(key)),
            _i18Nresolver.I18NValue("dashboard.actionrequiredopenrequests_tooltip",
                c.GetDefaultI18NValue(key, true)));


            _dashboards[c.ActionRequiredForOpenRequestsOffering] = DashboardDefinition.Get5ElementsInstance(
           key, "offering", "dashboardList",
           _i18Nresolver.I18NValue("dashboard.actionrequiredopenrequests", c.GetDefaultI18NValue(key)),
           _i18Nresolver.I18NValue("dashboard.actionrequiredopenrequests_tooltip",
               c.GetDefaultI18NValue(key, true)));

            key = c.EUOpenRequests;
            _dashboards[key] = DashboardDefinition.Get5ElementsInstance(
                 key, "servicerequest", "dashboardList",
                 _i18Nresolver.I18NValue("dashboard.openrequests", c.GetDefaultI18NValue(key)),
                 _i18Nresolver.I18NValue("dashboard.openrequests_tooltip", c.GetDefaultI18NValue(key, true)));

            key = c.ActionRequiredForOpenIncidents;
            _dashboards[key] = DashboardDefinition.Get5ElementsInstance(
                key, "incident", "dashboardList",
                _i18Nresolver.I18NValue("dashboard.actionrequiredopenincidents", c.GetDefaultI18NValue(key)),
                _i18Nresolver.I18NValue("dashboard.actionrequiredopenincidents_tooltip", c.GetDefaultI18NValue(key, true)));

            key = c.OpenImacs;
            _dashboards[key] = DashboardDefinition.GetDefaultInstance(
                key, "imac", "imacDashboardList",
                _i18Nresolver.I18NValue("dashboard.openimacs", c.GetDefaultI18NValue(key)),
                _i18Nresolver.I18NValue("dashboard.openimacs_tooltip", c.GetDefaultI18NValue(key, true)));

            key = c.OpenApprovals;
            _dashboards[key] = DashboardDefinition.Get5ElementsInstance(
                key, "change", "changeApprovalsDashboardList",
                _i18Nresolver.I18NValue("dashboard.changeopenapprovals", c.GetDefaultI18NValue(key)),
                _i18Nresolver.I18NValue("dashboard.changeopenapprovals_tooltip", c.GetDefaultI18NValue(key, true)), "gridwithoutsrwithapprovals");

            key = c.OpenChangeTasks;
            _dashboards[key] = DashboardDefinition.Get5ElementsInstance(
                key, "woactivity", "changeTasksDashboardList",
                _i18Nresolver.I18NValue("dashboard.changeopentasks", c.GetDefaultI18NValue(key)),
                 _i18Nresolver.I18NValue("dashboard.changeopentasks_tooltip", c.GetDefaultI18NValue(key, true)), "change.gridwithoutsr", "change.detail", "parent");

        }
        //
        // GET: /HapagHome/
        public GenericResponseResult<IList<DashboardDefinition>> Get() {
            var user = SecurityFacade.CurrentUser();
            if (user.HasProfile(ProfileType.Itc)) {
                return GetFromDict(c.ActionRequiredForOpenRequests, c.ActionRequiredForOpenIncidents, c.OpenImacs);
            }
            return GetFromDict(c.ActionRequiredForOpenRequests, c.EUOpenRequests);
        }

        [HttpGet]
        public GenericResponseResult<IList<DashboardDefinition>> SSOHome() {
            return GetFromDict(c.ActionRequiredForOpenRequests, c.EUOpenRequests, c.ActionRequiredForOpenIncidents, c.OpenApprovals, c.OpenChangeTasks);
        }

        [HttpGet]
        public GenericResponseResult<IList<DashboardDefinition>> TUIHome() {
            return GetFromDict(c.ActionRequiredForOpenRequests, c.EUOpenRequests, c.ActionRequiredForOpenIncidents, c.OpenApprovals, c.OpenChangeTasks);
        }

        [HttpGet]
        public GenericResponseResult<IList<DashboardDefinition>> XITCHome() {
            return GetFromDict(c.ActionRequiredForOpenRequests, c.ActionRequiredForOpenIncidents, c.OpenImacs);
        }

        [HttpGet]
        public GenericResponseResult<IList<DashboardDefinition>> OfferingHome() {
            return GetFromDict(c.ActionRequiredForOpenRequestsOffering);
        }

        [HttpGet]
        public GenericResponseResult<IList<DashboardDefinition>> TomHome() {
            return GetFromDict(c.ActionRequiredForOpenRequests, c.ActionRequiredForOpenIncidents, c.OpenApprovals, c.OpenChangeTasks);
        }

        [HttpGet]
        public GenericResponseResult<IList<DashboardDefinition>> ITomHome() {
            return GetFromDict(c.ActionRequiredForOpenRequests, c.ActionRequiredForOpenIncidents, c.OpenApprovals, c.OpenChangeTasks);
        }

        [HttpGet]
        public GenericResponseResult<IList<DashboardDefinition>> ADHome() {
            return GetFromDict(c.ActionRequiredForOpenIncidents);
        }

        [HttpGet]
        public GenericResponseResult<IList<DashboardDefinition>> ChangeHome() {
            return GetFromDict(c.OpenApprovals, c.OpenChangeTasks);
        }



        private GenericResponseResult<IList<DashboardDefinition>> GetFromDict(params string[] dashboards) {
            return DoGetFromList(dashboards.Select(dashboard => _dashboards[dashboard]).ToList());
        }

        private static void DashboardModuleHandler(IEnumerable<DashboardDefinition> dashboardDefinitionList) {
            if (string.IsNullOrEmpty(ContextManager.LookupContext().Module)) {
                return;
            }
            foreach (var dashboardDefinition in dashboardDefinitionList) {
                dashboardDefinition.Title = c.GetDefaultI18NValue(dashboardDefinition.Id);
                dashboardDefinition.Tooltip = c.GetDefaultI18NValue(dashboardDefinition.Id, true);
            }
        }

        private static GenericResponseResult<IList<DashboardDefinition>> DoGetFromList(List<DashboardDefinition> dashboardDefinitionList) {
            InMemoryUser user = SecurityFacade.CurrentUser();
            var dataObjectSet = new BaseApplicationDataSet();

            DashboardModuleHandler(dashboardDefinitionList);

            foreach (var definition in dashboardDefinitionList) {
                var key = new ApplicationMetadataSchemaKey(definition.SchemaId, definition.Mode, ClientPlatform.Web);
                var searchRequestDto = new SearchRequestDto {
                    SearchParams = definition.SearchParams,
                    SearchValues = definition.SearchValues,
                    Context = new ApplicationLookupContext { MetadataId = definition.Id }
                };
                var applicationMetadata = MetadataProvider.Application(definition.ApplicationName)
                    .ApplyPolicies(key, user, ClientPlatform.Web);

                definition.TotalCount = dataObjectSet.GetCount(applicationMetadata, searchRequestDto);
            }
            return new GenericResponseResult<IList<DashboardDefinition>>(dashboardDefinitionList, null) {
                Title = new I18NResolver().I18NValue("_headermenu.serviceit", "ServiceIT")
            };
        }
    }
}
