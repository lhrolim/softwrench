using System.Collections.Generic;
using System.Web.Http;
using cts.commons.web.Attributes;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers.Security {
    [Authorize]
    public class RoleController : ApiController {
        private static readonly SecurityFacade SecurityFacade = SecurityFacade.GetInstance();

        private readonly SWDBHibernateDAO _dao;

        public RoleController(SWDBHibernateDAO dao)
        {
            this._dao = dao;
        }


        [SPFRedirect("Role Setup", "_headermenu.rolesetup")]
        public GenericResponseResult<IList<Role>> Get() {
            var roles = _dao.FindByQuery<Role>("from Role order by name");
            return new GenericResponseResult<IList<Role>>(roles);
        }

        public GenericResponseResult<IList<Role>> Post(Role role) {
            SecurityFacade.SaveUpdateRole(role);

            var response = new GenericResponseResult<IList<Role>> {
                ResultObject = Get().ResultObject,
                SuccessMessage = "Role successfully saved"
            };

            return response;
        }

        [HttpDelete]
        public GenericResponseResult<IList<Role>> Delete(Role role) {
            SecurityFacade.DeleteRole(role);

            var response = new GenericResponseResult<IList<Role>> {
                ResultObject = Get().ResultObject,
                SuccessMessage = "Role successfully deleted"
            };

            return response;
        }

        public GenericResponseResult<IList<Role>> Put(Role role) {
            SecurityFacade.DeleteRole(role);

            var response = new GenericResponseResult<IList<Role>> {
                ResultObject = Get().ResultObject,
                SuccessMessage = "Role successfully deleted"
            };

            return response;
        }
    }
}