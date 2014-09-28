using System;
using System.Linq;
using Iesi.Collections.Generic;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector.Events;
using softWrench.sW4.Util;

namespace softWrench.sW4.Security.Init {
    class DbUserInitializer : ISWEventListener<ApplicationStartedEvent> {
        private static readonly SecurityFacade SecurityFacade = SecurityFacade.GetInstance();
        private static SWDBHibernateDAO _dao;

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
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
                            if (user == null && (ApplicationConfiguration.IsDev() || ApplicationConfiguration.ClientName!="hapag")) {
                                var adminUser = new User("swadmin", "admin", "admin", ApplicationConfiguration.DefaultSiteId ?? ApplicationConfiguration.DefaultOrgId,
                                    ApplicationConfiguration.DefaultOrgId ?? "ble", "test", "1-800-433-7300", "en", "sw@dm1n");
                                _dao.Save(adminUser);
                                CreateUserRoles(adminUser, UserType.Admin);
                            }
                            break;
                        case UserType.Job:
                            user = _dao.FindSingleByQuery<User>(User.UserByUserName, JobManager.JobUser);
                            if (user == null && (ApplicationConfiguration.IsDev() || ApplicationConfiguration.ClientName != "hapag")) {
                                var jobUser = new User(JobManager.JobUser, "jobuser", "jobuser", ApplicationConfiguration.DefaultSiteId ?? "bla",
                                    ApplicationConfiguration.DefaultOrgId ?? "ble", "test", "1-800-433-7300", "en", null);
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
                        Name = sysRole
                    });
                }
                var role2 = _dao.FindSingleByQuery<Role>(Role.RoleByName, clientRole);
                if (role2 == null) {
                    role2 = _dao.Save(new Role {
                        Name = clientRole
                    });
                }
                var userCustomRoles = new HashedSet<UserCustomRole>
                {
                    new UserCustomRole {Exclusion = false, Role = role},
                    new UserCustomRole {Exclusion = false, Role = role2}
                };
                SecurityFacade.SaveUser(user, null, userCustomRoles, null);
            }
        }

        private static void UpdateOrgId(User user) {
            var defaultOrgId = ApplicationConfiguration.DefaultOrgId;
            if (defaultOrgId == null) return;
            if (user.OrgId == defaultOrgId) return;
            user.OrgId = defaultOrgId;
            _dao.Save(user);
        }

        private static void UpdateSiteId(User user) {
            var defaultSiteId = ApplicationConfiguration.DefaultSiteId;
            if (defaultSiteId == null) return;
            if (user.SiteId == defaultSiteId) return;
            user.SiteId = defaultSiteId;
            _dao.Save(user);
        }
    }
}
