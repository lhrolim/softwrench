using System.Collections.Generic;
using System.Web.Http;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers.Security {
    [Authorize]
    public class RoleController : ApiController {
        private readonly SWDBHibernateDAO _dao;

        public RoleController(SWDBHibernateDAO dao) {
            this._dao = dao;
        }

        [SPFRedirect("Application Permissions", "_headermenu.rolesetup")]
        public GenericResponseResult<IList<Role>> Get() {
            var roles = _dao.FindByQuery<Role>("from Role order by name");
            return new GenericResponseResult<IList<Role>>(roles);
        }

        public GenericResponseResult<IList<Role>> Post(Role role) {
            SecurityFacade.GetInstance().SaveUpdateRole(role);

            var response = new GenericResponseResult<IList<Role>> {
                ResultObject = Get().ResultObject,
                SuccessMessage = "Role successfully saved"
            };

            return response;
        }

        [HttpDelete]
        public GenericResponseResult<IList<Role>> Delete(Role role) {
            SecurityFacade.GetInstance().DeleteRole(role);

            var response = new GenericResponseResult<IList<Role>> {
                ResultObject = Get().ResultObject,
                SuccessMessage = "Role successfully deleted"
            };

            return response;
        }

        public GenericResponseResult<IList<Role>> Put(Role role) {
            SecurityFacade.GetInstance().DeleteRole(role);

            var response = new GenericResponseResult<IList<Role>> {
                ResultObject = Get().ResultObject,
                SuccessMessage = "Role successfully deleted"
            };

            return response;
        }
    }
}