using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Web.Http;
using cts.commons.simpleinjector.Events;
using cts.commons.web.Attributes;
using softwrench.sw4.dashboard.classes.model;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata;
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
        public IGenericResponseResult SaveDashboard([FromUri]Dashboard dashboard, [FromUri]string policy) {
            //TODO: update menu, clear caching
            var user = SecurityFacade.CurrentUser();
            if ("personal".Equals(policy)) {
                dashboard.Filter = new DashboardFilter();
                dashboard.Filter.UserId = user.UserId;
            }
            user.Genericproperties[DashboardConstants.DashBoardsProperty] = null;
            _dispatcher.Dispatch(new ClearMenuEvent());
            return new GenericResponseResult<Dashboard>(_dao.Save(dashboard));
        }


        [HttpGet]
        public IGenericResponseResult Manage() {
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

            var names = MetadataProvider.Applications().Select(a => a.ApplicationName);
            IList<IAssociationOption> applications = names.Select(name => new GenericAssociationOption(name, name)).Cast<IAssociationOption>().ToList();

            if (user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsPreferredProperty)) {
                preferredDashboardId = user.Genericproperties[DashboardConstants.DashBoardsPreferredProperty] as int?;
            }
            if (user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsProperty)) {
                dashboards = (IEnumerable<Dashboard>)user.Genericproperties[DashboardConstants.DashBoardsProperty];
            }
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
            //TODO: add id checkings on server side
            var user = SecurityFacade.CurrentUser();

            int? preferredDashboardId = null;
            var dashboard = new Dashboard();


            if (user.Genericproperties.ContainsKey((DashboardConstants.DashBoardsPreferredProperty))) {
                // Get prefer dashboard id
                preferredDashboardId = user.Genericproperties[DashboardConstants.DashBoardsPreferredProperty] as int?;
            }

            if (user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsProperty)) {
                IEnumerable<Dashboard> dashboards = (IEnumerable<Dashboard>)user.Genericproperties[DashboardConstants.DashBoardsProperty];

                // Return the prefer dashboard content
                if (preferredDashboardId != null) {
                    dashboard = dashboards.FirstOrDefault(s => s.Id == preferredDashboardId);
                } else {
                    // TODO: Default it to global dashboard??        
                }
            } else {
                // Will it ever occur that we have a dashboard that's not in the Genericproperties? 
            }

            return new GenericResponseResult<Dashboard>(dashboard);
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
