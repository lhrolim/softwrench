using System.Collections.Generic;
using System.Web.Http;
using cts.commons.web.Attributes;
using softwrench.sw4.dashboard.classes.model;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SPF;

namespace softwrench.sw4.dashboard.classes.controller {

    [Authorize]
    [SPFRedirect(URL = "Application",CrudSubTemplate="/Shared/dashboard/templates/Dashboard.html")]
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
            var detailSchema =app.Schema(new ApplicationMetadataSchemaKey("detail"));


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
                NewPanelSchema = detailSchema
            };

            return new GenericResponseResult<ManageDashBoardsDTO>(dto);
        }

        public IGenericResponseResult LoadPreferred() {
            //TODO: add id checkings on server side
            var user = SecurityFacade.CurrentUser();
            var dashboard = user.Genericproperties["dashboard_preferred"];
            return new GenericResponseResult<Dashboard>(dashboard as Dashboard);
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
