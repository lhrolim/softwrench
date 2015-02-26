using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Web.Http;
using cts.commons.web.Attributes;
using softwrench.sw4.dashboard.classes.model;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SPF;

namespace softwrench.sw4.dashboard.classes.controller {

    [Authorize]
    [SPFRedirect(URL = "Application", CrudSubTemplate = "/Shared/dashboard/templates/Dashboard.html")]
    [SWControllerConfiguration]
    public class DashBoardController : ApiController {

        private readonly SWDBHibernateDAO _dao;


        public DashBoardController(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        [HttpPost]
        public IGenericResponseResult AddDashBoard(DashboardBasePanel dashBoardPanel) {
            //TODO: add id checkings on server side
            var userId = SecurityFacade.CurrentUser().DBId;
            return new BlankApplicationResponse();
        }


        [HttpGet]
        public IGenericResponseResult Manage() {
            //TODO: add id checkings on server side
            var user = SecurityFacade.CurrentUser();
            int? preferredDashboardId = null;
            IEnumerable<Dashboard> dashboards = null;
            var canCreateShared = user.IsInRole(DashboardConstants.RoleAdmin);
            var canCreateOwn = user.IsInRole(DashboardConstants.RoleManager);

            var app = MetadataProvider.Application("_dashboardgrid");
            var detailSchema = app.Schema(new ApplicationMetadataSchemaKey("detail"));

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
                NewPanelSchema = detailSchema,
                Applications = applications
            };

            return new GenericResponseResult<ManageDashBoardsDTO>(dto);
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

            if (user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsProperty))
            {
                IEnumerable<Dashboard> dashboards = (IEnumerable<Dashboard>)user.Genericproperties[DashboardConstants.DashBoardsProperty];

                // Return the prefer dashboard content
                if (preferredDashboardId != null) {
                    dashboard = dashboards.FirstOrDefault(s => s.Id == preferredDashboardId);
                }
                else {
                    // TODO: Default it to global dashboard??        
                }
            }
            else {
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
