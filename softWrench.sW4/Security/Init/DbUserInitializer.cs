using System;
using System.Linq;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.simpleinjector.Core.Order;
using Iesi.Collections.Generic;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Security.Init {
    public class DbUserInitializer : ISWEventListener<ApplicationStartedEvent>, IPriorityOrdered {
        private static SWDBHibernateDAO _dao;

        public int Order {
            get {
                return 4;
            }
        }

        [Transactional(DBType.Swdb)]
        public virtual void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            CreateUser();
        }

        public DbUserInitializer(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        private static void CreateUser() {
            try {
                var defaultUsers = Enum.GetValues(typeof(UserType)).Cast<UserType>();
                foreach (var defaultUser in defaultUsers) {
                    User user = null;
                    switch (defaultUser) {
                        case UserType.Admin:
                        user = _dao.FindSingleByQuery<User>(User.UserByUserName, "swadmin");
                        if (user == null && (ApplicationConfiguration.IsDev() || ApplicationConfiguration.ClientName != "hapag")) {
                            var adminUser = User.CreateAdminUser("swadmin", "admin", "admin", ApplicationConfiguration.DefaultSiteId ?? ApplicationConfiguration.DefaultOrgId,
                                ApplicationConfiguration.DefaultOrgId ?? "ble", "test", "1-800-433-7300", "en", "sw@dm1n", ApplicationConfiguration.DefaultStoreloc, "swadmin@controltechnologysolutions.com");
                            adminUser.MaximoPersonId = "swadmin";
                            adminUser.Systemuser = true;
                            _dao.Save(adminUser);
                            CreateUserRoles(adminUser, UserType.Admin);
                        }
                        break;
                        case UserType.Job:
                        user = _dao.FindSingleByQuery<User>(User.UserByUserName, JobManager.JobUser);
                        if (user == null && (ApplicationConfiguration.IsDev() || ApplicationConfiguration.ClientName != "hapag")) {
                            var jobUser = User.CreateAdminUser(JobManager.JobUser, "jobuser", "jobuser", ApplicationConfiguration.DefaultSiteId ?? "bla",
                                ApplicationConfiguration.DefaultOrgId ?? "ble", "test", "1-800-433-7300", "en", null, ApplicationConfiguration.DefaultStoreloc, "swadmin@controltechnologysolutions.com");
                            jobUser.MaximoPersonId = "jobuser";
                            jobUser.Systemuser = true;
                            _dao.Save(jobUser);
                            CreateUserRoles(jobUser, UserType.Job);
                        }
                        break;
                    }
                    if (user == null) {
                        continue;
                    }
                    UpdateOrgId(user);
                    UpdateSiteId(user);
                    UpdateStoreloc(user);
                }
            } catch (Exception e) {
                Console.Write(e.StackTrace);
            }
        }

        private static void CreateUserRoles(User user, UserType userType) {
            var sysRole = userType == UserType.Admin
            ? Role.SysAdmin
            : userType == UserType.Job
            ? Role.SysJob
            : null;

            var clientRole = userType == UserType.Admin
            ? Role.ClientAdmin
            : userType == UserType.Job
            ? Role.ClientJob
            : null;

            if (sysRole != null && clientRole != null) {
                var role = _dao.FindSingleByQuery<Role>(Role.RoleByName, sysRole);
                if (role == null) {
                    role = _dao.Save(new Role {
                        Name = sysRole,
                        Active = true
                    });
                }
                var role2 = _dao.FindSingleByQuery<Role>(Role.RoleByName, clientRole);
                if (role2 == null) {
                    role2 = _dao.Save(new Role {
                        Name = clientRole,
                        Active = true
                    });
                }
                var userCustomRoles = new LinkedHashSet<UserCustomRole>
                {
                    new UserCustomRole {Exclusion = false, Role = role},
                    new UserCustomRole {Exclusion = false, Role = role2}
                };
                SecurityFacade.GetInstance().SaveUser(user, null, userCustomRoles, null);
            }
        }

        private static void UpdateOrgId(User user) {
            var defaultOrgId = ApplicationConfiguration.DefaultOrgId;
            if (defaultOrgId == null) return;
            if (user.Person.OrgId == defaultOrgId) return;
            user.Person.OrgId = defaultOrgId;
            _dao.Save(user);
        }

        private static void UpdateSiteId(User user) {
            var defaultSiteId = ApplicationConfiguration.DefaultSiteId;
            if (defaultSiteId == null) return;
            if (user.Person.SiteId == defaultSiteId) return;
            user.Person.SiteId = defaultSiteId;
            _dao.Save(user);
        }

        private static void UpdateStoreloc(User user) {
            var defaultStoreloc = ApplicationConfiguration.DefaultStoreloc;
            if (defaultStoreloc == null) return;
            if (user.Person.Storeloc == defaultStoreloc) return;
            user.Person.Storeloc = defaultStoreloc;
            _dao.Save(user);
        }
    }
}
