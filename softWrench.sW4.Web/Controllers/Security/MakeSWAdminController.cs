using System.Web.Http;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers.Security {
    [System.Web.Mvc.Authorize]
    public class MakeSWAdminController : ApiController {

        private static SWDBHibernateDAO _dao;

        public MakeSWAdminController(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        [HttpGet]
        [SPFRedirect(URL = "MakeSWAdmin", Title = "Make SW Admin")]
        public IGenericResponseResult Index() {
            return new RedirectResponseResult();
        }

        [HttpGet]
        public IGenericResponseResult Submit(string password) {
            var user = SecurityFacade.CurrentUser();
            var authorized = false;
            var adminUser = SWDBHibernateDAO.GetInstance().FindSingleByQuery<User>(sW4.Security.Entities.User.UserByUserName, "swadmin");
            if (adminUser.Password != null) {
                var authenticatedAdminUser = SecurityFacade.GetInstance().Login(adminUser, password, string.Empty);
                if (authenticatedAdminUser != null) {
                    if (!user.IsInRole(Role.SysAdmin)) {
                        var adminRole = _dao.FindSingleByQuery<Role>(Role.RoleByName, Role.SysAdmin);
                        user.Roles.Add(adminRole);
                    }
                    if (!user.IsInRole(Role.ClientAdmin)) {
                        var clientRole = _dao.FindSingleByQuery<Role>(Role.RoleByName, Role.ClientAdmin);
                        user.Roles.Add(clientRole);
                    }
                    authorized = true;
                }
            }
            return new GenericResponseResult<bool>(authorized);
        }
    }
}
