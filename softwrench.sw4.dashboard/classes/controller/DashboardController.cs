using System.Web.Http;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softwrench.sw4.dashboard.classes.controller {
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
            var userId = SecurityFacade.CurrentUser().DBId;
            return new BlankApplicationResponse();
        }

        public IGenericResponseResult LoadPreferred() {
            //TODO: add id checkings on server side
            var user =SecurityFacade.CurrentUser();
            var dashboard =user.Genericproperties["dashboard_preferred"];
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
