using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Web.Http;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.Events;
using cts.commons.web.Attributes;
using softwrench.sw4.dashboard.classes.model;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SPF;

namespace softwrench.sw4.dashboard.classes.controller {

    [Authorize]
    [SPFRedirect(URL = "Application", CrudSubTemplate = "/Shared/dashboard/templates/Dashboard.html")]
    [SWControllerConfiguration]
    public class DashBoardController : ApiController {

        private readonly SWDBHibernateDAO _dao;

        private readonly UserDashboardManager _userDashboardManager;
        private IEventDispatcher _dispatcher;

        public DashBoardController(SWDBHibernateDAO dao, UserDashboardManager userDashboardManager, IEventDispatcher dispatcher) {
            _dao = dao;
            _userDashboardManager = userDashboardManager;
            _dispatcher = dispatcher;
        }

        [HttpPost]
        public IGenericResponseResult SaveDashboard(Dashboard dashboard) {
            //TODO: update menu, clear caching
            var user = SecurityFacade.CurrentUser();
            var currentdtm = DateTime.Now;

//            if ("personal".Equals(policy)) {
//                dashboard.Filter = new DashboardFilter {
//                    UserId = user.UserId
//                };
//            }

            // Populate default values
            if (dashboard.Layout == null) {
                dashboard.Layout = "0";
            }

            var savedDashboard = _dao.Save(dashboard);
            user.Genericproperties.Remove(DashboardConstants.DashBoardsProperty);
            _dispatcher.Dispatch(new ClearMenuEvent());
            return new GenericResponseResult<Dashboard>(savedDashboard);
        }


        [HttpGet]
        public GenericResponseResult<ManageDashBoardsDTO> Manage() {
            //TODO: add id checkings on server side
            var user = SecurityFacade.CurrentUser();
            int? preferredDashboardId = null;
            IEnumerable<Dashboard> dashboards = null;
            var canCreateShared = user.IsInRole(DashboardConstants.RoleAdmin);
            var canCreateOwn = user.IsInRole(DashboardConstants.RoleManager);

            var panelSelectionSchema = MetadataProvider.Application("_basedashboard").Schema(new ApplicationMetadataSchemaKey("panelselection"));
            var saveDashboardSchema = MetadataProvider.Application("_dashboard").Schema(new ApplicationMetadataSchemaKey("saveDashboardConfirmation"));

            var panelSchemas = new Dictionary<string, ApplicationSchemaDefinition>();
            panelSchemas.Add("dashboardgrid", MetadataProvider.Application("_dashboardgrid").Schema(new ApplicationMetadataSchemaKey("detail")));

            var profiles = SecurityFacade.GetInstance()
                .FetchAllProfiles(false)
                .Select(p => new GenericAssociationOption(p.Id.ToString(), p.Name))
                .Cast<IAssociationOption>()
                .ToList();

            var topLevel = MetadataProvider.FetchTopLevelApps(ClientPlatform.Web,user);
            var securedMetadatas = topLevel.Select(metadata => metadata.CloneSecuring(user)).ToList();
            var names = securedMetadatas.Select(a => a.ApplicationName);
            IList<IAssociationOption> applications = names.Select(name => new GenericAssociationOption(name, name)).Cast<IAssociationOption>().ToList();

            if (user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsPreferredProperty)) {
                preferredDashboardId = user.Genericproperties[DashboardConstants.DashBoardsPreferredProperty] as int?;
            }
            if (!user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsProperty)) {
                user.Genericproperties[DashboardConstants.DashBoardsProperty] = _userDashboardManager.LoadUserDashboars(user);
            }
            dashboards = (IEnumerable<Dashboard>)user.Genericproperties[DashboardConstants.DashBoardsProperty];
            var dto = new ManageDashBoardsDTO() {
                CanCreateOwn = canCreateOwn,
                CanCreateShared = canCreateShared,
                Dashboards = dashboards,
                PreferredId = preferredDashboardId,
                NewPanelSchema = panelSelectionSchema,
                PanelSchemas = panelSchemas,
                SaveDashboardSchema = saveDashboardSchema,
                Applications = applications,
                Profiles = profiles
            };

            return new GenericResponseResult<ManageDashBoardsDTO>(dto);
        }

        [HttpGet]
        public IGenericResponseResult LoadFields([FromUri]String applicationName) {
            var app = MetadataProvider.Application(applicationName);
            ApplicationSchemaDefinition schema = app.GetListSchema();
            var options = schema.Fields.Select(f => new GenericAssociationOption(f.Attribute, f.Label)).Where(f => !string.IsNullOrEmpty(f.Label))
                .Cast<IAssociationOption>()
                .ToList();
            return new GenericResponseResult<IEnumerable<IAssociationOption>>(options);
        }



        [HttpGet]
        public IGenericResponseResult LoadPanel([FromUri]String panel) {
            return new GenericResponseResult<DashboardBasePanel>(_dao.FindByPK<DashboardBasePanel>(typeof(DashboardBasePanel), Int32.Parse(panel)));
        }

        [HttpGet]
        public IGenericResponseResult LoadPanels([FromUri]String paneltype) {
            var availablePanels = _userDashboardManager.LoadUserPanels(SecurityFacade.CurrentUser(), paneltype);
            var options = availablePanels.Select(f => new GenericAssociationOption(f.Id.ToString(), f.Alias))
                .Cast<IAssociationOption>()
                .ToList();
            return new GenericResponseResult<IEnumerable<IAssociationOption>>(options);
        }


        [HttpPost]
        public IGenericResponseResult CreatePanel([FromUri]DashboardGridPanel panel) {
            panel.Filter = new DashboardFilter();
            var app = MetadataProvider.Application(panel.Application);
            ApplicationSchemaDefinition schema = app.GetListSchema();
            panel.SchemaRef = schema.SchemaId;
            return new GenericResponseResult<DashboardBasePanel>(_dao.Save(panel));
        }

        [HttpGet]
        public IGenericResponseResult LoadPreferred() {
            var manageDTO = Manage();
            return manageDTO;
        }

        [HttpGet]
        public IGenericResponseResult LoadDashboard(int? dashBoardId) {
            var manageDTO = Manage();
            manageDTO.ResultObject.PreferredId = dashBoardId;
            return manageDTO;
        }

        private Dashboard DoLoadDashBoard(int? dashBoardId, InMemoryUser user) {
            if (!user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsProperty)) {
                // Get dashboard information and store into cache
                user.Genericproperties[DashboardConstants.DashBoardsProperty] = _userDashboardManager.LoadUserDashboars(user);
            }

            var dashboards = (IEnumerable<Dashboard>)user.Genericproperties[DashboardConstants.DashBoardsProperty];
            var enumerable = dashboards as Dashboard[] ?? dashboards.ToArray();
            if (dashboards == null || !enumerable.Any()) {
                return null;
            }
            if (dashBoardId == null) {
                //fallback to first one
                return enumerable.FirstOrDefault();
            }
            return enumerable.FirstOrDefault(s => s.Id == dashBoardId);
        }

        public IGenericResponseResult EditDashBoard(DashboardBasePanel dashBoardPanel) {
            //TODO: add id checkings on server side
            var userId = SecurityFacade.CurrentUser().DBId;
            return new BlankApplicationResponse();
        }

        public IGenericResponseResult RemoveDashBoard(int dashBoardId) {
            //TODO: add id checkings on server side
            var userId = SecurityFacade.CurrentUser().DBId;
            return new BlankApplicationResponse();
        }

        public IGenericResponseResult ModifyStructure(Dashboard dashboardPreferences) {
            //TODO: add id checkings on server side
            var userId = SecurityFacade.CurrentUser().DBId;
            return new BlankApplicationResponse();
        }
    }
}
