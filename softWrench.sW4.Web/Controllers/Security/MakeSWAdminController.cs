using System.Web.Http;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.services;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers.Security {
    [System.Web.Mvc.Authorize]
    public class MakeSWAdminController : ApiController {

        private readonly SWDBHibernateDAO _dao;
        private readonly UserPasswordService _passwordService;

        public MakeSWAdminController(SWDBHibernateDAO dao, UserPasswordService passwordService)
        {
            _dao = dao;
            _passwordService = passwordService;
        }

        [HttpGet]
        [SPFRedirect(URL = "MakeSWAdmin", Title = "Make SW Admin")]
        public IGenericResponseResult Index() {
            return new RedirectResponseResult();
        }

        [HttpPost]
        public IGenericResponseResult Submit([FromBody]PasswordDTO passwordDTO) {
            var authorized = false;
            var adminUser = SWDBHibernateDAO.GetInstance().FindSingleByQuery<User>(softwrench.sw4.user.classes.entities.User.UserByUserName, "swadmin");

            if (adminUser.Password == null) {
                return new GenericResponseResult<bool>(false);
            }

            if (passwordDTO.NewMasterPassword != null) {
                _passwordService.DefineMasterPassword(passwordDTO.NewMasterPassword);
            }

            var authenticatedAdminUser = SecurityFacade.GetInstance().LoginCheckingPassword(adminUser, passwordDTO.Password, string.Empty);
            if (authenticatedAdminUser != null) {
                var user = SecurityFacade.CurrentUser();

                if (!user.IsInRole(Role.SysAdmin)) {
                    var adminRole = _dao.FindSingleByQuery<Role>(Role.RoleByName, Role.SysAdmin);
                    user.Roles.Add(adminRole);
                }
                if (!user.IsInRole(Role.ClientAdmin)) {
                    var clientRole = _dao.FindSingleByQuery<Role>(Role.RoleByName, Role.ClientAdmin);
                    user.Roles.Add(clientRole);
                }
                if (!user.IsInRolInternal(Role.DynamicAdmin, false)) {
                    var dynamicRole = new Role() {
                        Active = true,
                        Deletable = false,
                        Description = Role.DynamicAdmin,
                        Id = 99999,
                        Label = Role.DynamicAdmin,
                        Name = Role.DynamicAdmin
                    };
                    user.Roles.Add(dynamicRole);
                }

                authorized = true;
            }

            return new GenericResponseResult<bool>(authorized);
        }

        public class PasswordDTO {
            public string Password {get; set;}
            public string NewMasterPassword {get; set;}
        }
    }
}
