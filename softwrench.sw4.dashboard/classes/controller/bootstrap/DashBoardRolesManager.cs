using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.dashboard.classes.model;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Entities;

namespace softwrench.sw4.dashboard.classes.controller.bootstrap {


    public class DashBoardRolesManager : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private readonly SWDBHibernateDAO _dao;

        public DashBoardRolesManager(SWDBHibernateDAO dao) {
            _dao = dao;
        }


        public void CreateRoles() {

            var roles = _dao.FindByQuery<Role>(Role.RoleByNames, new List<string> { DashboardConstants.RoleAdmin, DashboardConstants.RoleManager });

            if (!roles.Any(r => r.Name.Equals(DashboardConstants.RoleAdmin))) {
                var role = new Role {
                    Active = true,
                    Deletable = false,
                    Description =
                        "Allows the creation/removal of other shareable dashboards for the entire organization, plus personal ones",
                    Name = DashboardConstants.RoleAdmin,
                    Label = "Dashboard Admin"
                };
                _dao.Save(role);
            }

            if (!roles.Any(r => r.Name.Equals(DashboardConstants.RoleManager))) {
                var role = new Role {
                    Active = true,
                    Deletable = false,
                    Description = "Allows the creation of personal dashboards, but do not allow the creation/removal of eventual shared ones",
                    Name = DashboardConstants.RoleManager,
                    Label = "Dashboard Manager"
                };
                _dao.Save(role);
            }
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            CreateRoles();
        }
    }
}
