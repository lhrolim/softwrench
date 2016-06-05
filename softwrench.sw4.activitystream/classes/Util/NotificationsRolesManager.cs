using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softwrench.sw4.activitystream.classes.Util {


    public class NotificationsRolesManager : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        public const string NotificationsRole = "ROLE_NOTIFICATIONS";

        private readonly SWDBHibernateDAO _dao;

        public NotificationsRolesManager(SWDBHibernateDAO dao) {
            _dao = dao;
        }


        public void CreateRoles() {

            var roles = _dao.FindByQuery<Role>(Role.RoleByName, NotificationsRole);

            if (roles.Any(r => r.Name.Equals(NotificationsRole)))
            {
                return;
            }

            var role = new Role {
                Active = true,
                Deletable = false,
                Description = "Allows the view of the recent activity panel.",
                Name = NotificationsRole,
                Label = "Recent Activity View"
            };
            _dao.Save(role);
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            CreateRoles();
        }
    }
}
