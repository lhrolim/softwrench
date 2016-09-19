using System.Linq;
using cts.commons.web.Attributes;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softwrench.sw4.Hapag.Data;
using softwrench.sw4.Hapag.Data.Init;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models.Hapag;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using c = softwrench.sw4.Hapag.Data.HapagDashBoardsConstants;

namespace softWrench.sW4.Web.Controllers {

    [System.Web.Mvc.Authorize]
    [SPFRedirect(URL = "Application", CrudSubTemplate = "/Templates/hapag/HapagHome.html")]
    [SWControllerConfiguration]
    public class HapagHomeController : ApiController {

        private readonly IConfigurationFacade _configFacade;
        private IWhereClauseFacade _whereClauseFacade;
        private readonly I18NResolver _i18Nresolver;

        protected static IContextLookuper ContextManager {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<IContextLookuper>(typeof(IContextLookuper));
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
        public async Task<GenericResponseResult<IList<DashboardDefinition>>> Get() {
            var user = SecurityFacade.CurrentUser();
            if (user.HasProfile(ProfileType.Itc)) {
                return await GetFromDict(c.ActionRequiredForOpenRequests, c.ActionRequiredForOpenIncidents, c.OpenImacs);
            }
            return await GetFromDict(c.ActionRequiredForOpenRequests, c.EUOpenRequests);
        }

        [HttpGet]
        public async Task<GenericResponseResult<IList<DashboardDefinition>>> XITCHome() {
            return await GetFromDict(c.ActionRequiredForOpenRequests, c.ActionRequiredForOpenIncidents, c.OpenImacs);
        }

        [HttpGet]
        public async Task<GenericResponseResult<IList<DashboardDefinition>>> TomHome() {
            return await GetFromDict(c.ActionRequiredForOpenRequests, c.ActionRequiredForOpenIncidents, c.OpenApprovals, c.OpenChangeTasks);
        }

        [HttpGet]
        public async Task<GenericResponseResult<IList<DashboardDefinition>>> ITomHome() {
            return await GetFromDict(c.ActionRequiredForOpenRequests, c.ActionRequiredForOpenIncidents, c.OpenApprovals, c.OpenChangeTasks);
        }

        [HttpGet]
        public async Task<GenericResponseResult<IList<DashboardDefinition>>> ADHome() {
            return await GetFromDict(c.ActionRequiredForOpenIncidents);
        }

        [HttpGet]
        public async Task<GenericResponseResult<IList<DashboardDefinition>>> ChangeHome() {
            return await GetFromDict(c.OpenApprovals, c.OpenChangeTasks);
        }

        [HttpGet]
        public async Task<GenericResponseResult<IList<DashboardDefinition>>> SSOHome() {
            return await GetFromDict(c.ActionRequiredForOpenRequests, c.EUOpenRequests, c.ActionRequiredForOpenIncidents, c.OpenApprovals, c.OpenChangeTasks);
        }

        [HttpGet]
        public async Task<GenericResponseResult<IList<DashboardDefinition>>> TUIHome() {
            return await GetFromDict(c.OpenApprovals, c.OpenChangeTasks);
        }

        private async Task<GenericResponseResult<IList<DashboardDefinition>>> GetFromDict(params string[] dashboards) {
            return await DoGetFromList(dashboards.Select(dashboard => _dashboards[dashboard]).ToList());
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

        private static async Task<GenericResponseResult<IList<DashboardDefinition>>> DoGetFromList(List<DashboardDefinition> dashboardDefinitionList) {
            var user = SecurityFacade.CurrentUser();
            var dataObjectSet = new MaximoApplicationDataSet();

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

                definition.TotalCount = await dataObjectSet.GetCount(applicationMetadata, searchRequestDto);
            }
            return new GenericResponseResult<IList<DashboardDefinition>>(dashboardDefinitionList, null) {
                Title = new I18NResolver().I18NValue("_headermenu.serviceit", "ServiceIT")
            };
        }
    }
}
