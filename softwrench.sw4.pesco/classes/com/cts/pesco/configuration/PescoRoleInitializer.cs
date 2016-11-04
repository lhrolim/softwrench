using System.Collections.Generic;
using cts.commons.persistence;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.dashboard.classes.model.entities;
using softwrench.sw4.dashboard.classes.startup;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Util;

namespace softwrench.sw4.pesco.classes.com.cts.pesco.configuration {
    public class PescoRoleInitializer : ISWEventListener<ApplicationStartedEvent>, IOrdered {


        private readonly ISWDBHibernateDAO _dao;

        public PescoRoleInitializer(ISWDBHibernateDAO dao) {
            _dao = dao;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var existingRole = _dao.FindSingleByQuery<Role>(Role.RoleByName, "worklogclientviewable");
            if (existingRole == null) {
                var role = new Role {
                    Name = "worklogclientviewable",
                    Active = true,
                    Deletable = false,
                    Description = "Allow Users to view worklogs which are not marked as clientviewables"
                };
                _dao.Save(role);
            }
        }


        public int Order {
            get {
                return ChartInitializer.ORDER + 1;
            }
        }
    }
}
