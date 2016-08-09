﻿using System.Web.Http;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.SWDB;
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
            var adminUser = SWDBHibernateDAO.GetInstance().FindSingleByQuery<User>(softwrench.sw4.user.classes.entities.User.UserByUserName, "swadmin");
            if (adminUser.Password != null) {
                var authenticatedAdminUser = SecurityFacade.GetInstance().LoginCheckingPassword(adminUser, password, string.Empty);
                if (authenticatedAdminUser != null) {
                    if (!user.IsInRole(Role.SysAdmin)) {
                        var adminRole = _dao.FindSingleByQuery<Role>(Role.RoleByName, Role.SysAdmin);
                        user.Roles.Add(adminRole);
                    }
                    if (!user.IsInRole(Role.ClientAdmin)) {
                        var clientRole = _dao.FindSingleByQuery<Role>(Role.RoleByName, Role.ClientAdmin);
                        user.Roles.Add(clientRole);
                    }
                    if (!user.IsInRole(Role.DynamicAdmin)) {
                        var dynamicRole = new Role() {
                            Active = true,
                            Deletable = true,
                            Description = "Dynamic admin role",
                            Id = 99999,
                            Label = "Dynamic Admin",
                            Name = "Dynamic Admin"  
                        }; 
                        user.Roles.Add(dynamicRole);
                    }

                    authorized = true;
                }
            }
            return new GenericResponseResult<bool>(authorized);
        }
    }
}
