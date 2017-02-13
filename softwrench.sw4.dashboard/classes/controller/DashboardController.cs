using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.simpleinjector.Events;
using cts.commons.web.Attributes;
using Iesi.Collections.Generic;
using softwrench.sw4.dashboard.classes.model;
using softwrench.sw4.dashboard.classes.model.entities;
using softwrench.sw4.dashboard.classes.service.graphic;
using softwrench.sw4.dashboard.classes.startup;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Security.Context;
using softWrench.sW4.SPF;

namespace softwrench.sw4.dashboard.classes.controller {

    [Authorize]
    [SPFRedirect(URL = "Application", CrudSubTemplate = "/Shared/dashboard/templates/Dashboard.html")]
    [SWControllerConfiguration]
    public class DashBoardController : ApiController {

        private readonly SWDBHibernateDAO _dao;
        private readonly UserDashboardManager _userDashboardManager;
        private readonly IEventDispatcher _dispatcher;
        private readonly GraphicStorageSystemFacadeProvider _graphicServiceProvider;

        private readonly IWhereClauseFacade _whereClauseFacade;

        public DashBoardController(SWDBHibernateDAO dao, UserDashboardManager userDashboardManager, IEventDispatcher dispatcher, GraphicStorageSystemFacadeProvider graphicServiceProvider,
            IWhereClauseFacade whereClauseFacade) {
            _dao = dao;
            _userDashboardManager = userDashboardManager;
            _dispatcher = dispatcher;
            _graphicServiceProvider = graphicServiceProvider;
            _whereClauseFacade = whereClauseFacade;
        }

        [HttpPost]
        public IGenericResponseResult SaveDashboard([FromBody]Dashboard dashboard) {
            //TODO: update menu, clear caching
            var user = SecurityFacade.CurrentUser();
            dashboard.PopulatePanelRelationshipsForStorage();

            Dashboard savedDashboard;
            dashboard.Active = true;

            if (dashboard.Id != null && dashboard.Cloning) {
                //copying from existing dashboard
                var dbDashBoard = _dao.FindByPK<Dashboard>(typeof(Dashboard), dashboard.Id);
                if (!dbDashBoard.Active) {
                    //reactivating already existing one
                    dbDashBoard.Active = true;
                    savedDashboard = _dao.Save(dbDashBoard);
                } else {
                    //cloning dashboard
                    dashboard.Id = null;
                    dashboard.Application = dbDashBoard.Application;
                    foreach (var panel in dbDashBoard.PanelsSet) {
                        //removing link id to force creation of a new one
                        panel.Id = null;
                        if (dashboard.PanelsSet == null) {
                            dashboard.PanelsSet = new LinkedHashSet<DashboardPanelRelationship>();
                        }
                        dashboard.PanelsSet.Add(panel);
                    }
                    savedDashboard = _dao.Save(dashboard);
                }
            } else {
                savedDashboard = _dao.Save(dashboard);
            }
            //enforcing that the cloing operation is done
            savedDashboard.Cloning = false;
            user.Genericproperties.Remove(DashboardConstants.DashBoardsProperty);
            _dispatcher.Dispatch(new ClearMenuEvent());
            return new GenericResponseResult<Dashboard>(savedDashboard);
        }

        [HttpPost]
        public IGenericResponseResult DeactivateDashboard([FromBody]Dashboard dashboard) {
            var user = SecurityFacade.CurrentUser();
            // check permission
            if (dashboard.CreatedBy != user.UserId && !user.IsInRole(DashboardConstants.RoleAdmin)) {
                throw new Exception("You do not have permission to deactivate this dashboard");
            }
            // deactivate and save
            dashboard.Active = false;
            _dao.Save(dashboard);
            // update cache
            user.Genericproperties.Remove(DashboardConstants.DashBoardsProperty);
            _dispatcher.Dispatch(new ClearMenuEvent());
            // black success response
            return new BlankApplicationResponse();
        }

        [HttpPost]
        public DashboardBasePanel SavePanel([FromBody]DashboardBasePanel panel) {
            var user = SecurityFacade.CurrentUser();
            var savedPanel = _dao.Save(panel);
            user.Genericproperties.Remove(DashboardConstants.DashBoardsProperty);

            return savedPanel;
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

            var panelSchemas = new Dictionary<string, ApplicationSchemaDefinition> {
                { "dashboardgrid", MetadataProvider.Application("_dashboardgrid").Schema(new ApplicationMetadataSchemaKey("detail")) },
                { "editdashboardgrid", MetadataProvider.Application("_dashboardgrid").Schema(new ApplicationMetadataSchemaKey("editdetail")) },
                { "dashboardgraphic", MetadataProvider.Application("_dashboardgraphic").Schema(new ApplicationMetadataSchemaKey("detail")) }
            };

            var profiles = SecurityFacade.GetInstance()
                .FetchAllProfiles(false)
                .Select(p => new GenericAssociationOption(p.Id.ToString(), p.Name))
                .Cast<IAssociationOption>()
                .ToList();

            var applicationNames = user.IsSwAdmin()
                // swadmin.MergedUserProfile has empty Permissions
                ? MetadataProvider.FetchSecuredTopLevelApps(ClientPlatform.Web, user).Select(a => a.ApplicationName)
                : user.MergedUserProfile.Permissions.Where(p => !p.HasNoPermissions).Select(p => p.ApplicationName);

            var applications = applicationNames.Select(name => new GenericAssociationOption(name, name))
                                            .Cast<IAssociationOption>()
                                            .ToList();
            applications.Sort();


            if (user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsPreferredProperty)) {
                preferredDashboardId = user.Genericproperties[DashboardConstants.DashBoardsPreferredProperty] as int?;
            }
            if (!user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsProperty)) {
                user.Genericproperties[DashboardConstants.DashBoardsProperty] = _userDashboardManager.LoadUserDashboars(user);
            }
            dashboards = (IEnumerable<Dashboard>)user.Genericproperties[DashboardConstants.DashBoardsProperty];
            var dto = new ManageDashBoardsDTO() {
                Permissions = new ManageDashBoardsDTO.ManageDashboardsPermissionDTO() {
                    CanCreateOwn = canCreateOwn,
                    CanCreateShared = canCreateShared,
                    CanDeleteOwn = canCreateOwn,
                    CanDeleteShared = canCreateShared
                },
                Schemas = new ManageDashBoardsDTO.ManageDashboardsSchemasDTO() {
                    NewPanelSchema = panelSelectionSchema,
                    PanelSchemas = panelSchemas,
                    SaveDashboardSchema = saveDashboardSchema,
                },
                Dashboards = dashboards,
                PreferredId = preferredDashboardId,
                Applications = applications,
                Profiles = profiles
            };

            return new GenericResponseResult<ManageDashBoardsDTO>(dto);
        }

        [HttpGet]
        public IGenericResponseResult LoadFields([FromUri]string applicationName) {
            var app = MetadataProvider.Application(applicationName);
            var schema = app.GetListSchema();
            if (schema == null) {
                //sometimes this method is getting called using _dashboard as application.
                //TODO: fix it.
                return new GenericResponseResult<IEnumerable<IAssociationOption>>(new List<IAssociationOption>());
            }

            var options = schema.Fields.Select(f => new GenericAssociationOption(f.Attribute, f.Label)).Where(f => !string.IsNullOrEmpty(f.Label))
                .Cast<IAssociationOption>()
                .ToList();
            return new GenericResponseResult<IEnumerable<IAssociationOption>>(options);
        }


        [HttpGet]
        public async Task<string> LoadPanelWhereClause([FromUri]string applicationName, [FromUri]string panelAlias) {
            var queryResult = await _whereClauseFacade.LookupAsync(applicationName, new ApplicationLookupContext() { MetadataId = "dashboard:" + panelAlias });
            return queryResult?.Query;
        }

        [HttpGet]
        public IGenericResponseResult LoadPanel([FromUri]string panel) {
            return new GenericResponseResult<DashboardBasePanel>(_dao.FindByPK<DashboardBasePanel>(typeof(DashboardBasePanel), int.Parse(panel)));
        }

        [HttpGet]
        public IGenericResponseResult LoadPanels([FromUri]string paneltype) {
            var availablePanels = _userDashboardManager.LoadUserPanels(SecurityFacade.CurrentUser(), paneltype);
            var options = availablePanels.Select(f => new GenericAssociationOption(f.Id.ToString(), f.Alias))
                .Cast<IAssociationOption>()
                .ToList();
            return new GenericResponseResult<IEnumerable<IAssociationOption>>(options);
        }






        [HttpPost]
        public async Task<IGenericResponseResult> SaveGridPanel(DashboardGridPanel panel) {
            var app = MetadataProvider.Application(panel.Application);
            var schema = app.GetListSchema();
            //TODO make it transactional

            await _whereClauseFacade.RegisterAsync(app.ApplicationName, panel.WhereClause, new WhereClauseRegisterCondition() {
                AppContext = new ApplicationLookupContext() {
                    MetadataId = "dashboard:" + panel.Alias
                }
            }, true);


            panel.SchemaRef = schema.SchemaId;
            panel.Filter = new DashboardFilter();
            var gridPanel = await _dao.SaveAsync(panel);
            gridPanel.WhereClause = panel.WhereClause;

            return new GenericResponseResult<DashboardBasePanel>(gridPanel);
        }

        [HttpPost]
        public async Task<IGenericResponseResult> SaveGraphicPanel(DashboardGraphicPanel panel) {
            panel.Filter = new DashboardFilter();
            await SaveGraphicDashWc(panel);
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

        /// <summary>
        /// Authenticates the user the selected graphic storage system provider.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IGenericResponseResult> Authenticate([FromUri]string provider, [FromBody] IDictionary<string, string> dto) {
            var service = _graphicServiceProvider.GetService(provider);
            var auth = await service.Authenticate(SecurityFacade.CurrentUser(), dto);
            return new GenericResponseResult<IGraphicStorageSystemAuthDto>(auth);
        }

        /// <summary>
        /// Fetches an external resource from the selected graphic storage system provider.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="resource"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> LoadGraphicResource([FromUri] string provider, [FromUri] string resource, [FromBody] IDictionary<string, string> dto) {
            var service = _graphicServiceProvider.GetService(provider);
            return await service.LoadExternalResource(resource, dto);
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

        private async Task SaveGraphicDashWc(DashboardGraphicPanel panel) {
            if (string.IsNullOrEmpty(panel.Configuration)) {
                throw new Exception("Widget configuration not found.");
            }

            var props = panel.Configuration.Split(';');
            var prop = props.FirstOrDefault(possibleProp => possibleProp.StartsWith("applicationName="));
            if (prop == null) {
                throw new Exception("Missing field 'Application' on widget configuration.");
            }
            var applicationName = prop.Replace("applicationName=", "");

            var metadataId = "dashboard:" + panel.Alias;
            var conditionAlias = ChartInitializer.AliasMetadataIdDict.ContainsKey(metadataId) ? ChartInitializer.AliasMetadataIdDict[metadataId] : metadataId;

            await _whereClauseFacade.RegisterAsync(applicationName, panel.WhereClause, new WhereClauseRegisterCondition() {
                Alias = conditionAlias,
                AppContext = new ApplicationLookupContext() {
                    MetadataId = metadataId
                }
            }, true);
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
