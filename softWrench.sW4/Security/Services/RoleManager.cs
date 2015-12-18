using cts.commons.Util;
using log4net;
using NHibernate;
using softWrench.sW4.Data.Persistence.SWDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using softwrench.sw4.user.classes.entities;

namespace softWrench.sW4.Security.Services {
    public class RoleManager {

        private static readonly ILog Log = LogManager.GetLogger(typeof(RoleManager));


        private static ISet<Role> _activeRoles = new HashSet<Role>();

        public static void LoadActiveRoles() {
            var before = Stopwatch.StartNew();
            var activeRoles = SWDBHibernateDAO.GetInstance().FindByQuery<Role>("from Role where Active = 1");
            _activeRoles = new HashSet<Role>(activeRoles);
            Log.Info(LoggingUtil.BaseDurationMessage( "Active Roles Loaded in {0}", before));
        }

        public static IList<String> ActiveFieldRoles() {
            return (from activeRole in _activeRoles select activeRole.Name).ToList();
        }

        public static IList<String> ActiveApplicationRoles() {
            return (from activeRole in _activeRoles where !activeRole.Name.Contains(".") select activeRole.Name).ToList();
        }

        public static Role SaveUpdateRole(Role role) {
            bool updatingRole = role.Id != null;
            role = SWDBHibernateDAO.GetInstance().Save(role);
            if (updatingRole) {
                Role oldRole = _activeRoles.FirstOrDefault(r => r.Id == role.Id);
                if (oldRole != null) {
                    _activeRoles.Remove(oldRole);
                }
            }
            if (role.Active) {
                _activeRoles.Add(role);
            }

            return role;
        }

        public static void DeleteRole(Role role)
        {
            var dao = SWDBHibernateDAO.GetInstance();
            using (ISession session = SWDBHibernateDAO.GetInstance().GetSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    dao.ExecuteSql("delete from sw_userprofile_role where role_id = ? ", role.Id);
                    dao.ExecuteSql("delete from sw_user_customrole where role_id = ? ",role.Id);
                    dao.Delete(role);
                    Role oldRole = _activeRoles.FirstOrDefault(r => r.Id == role.Id);
                    if (oldRole != null) {
                        _activeRoles.Remove(oldRole);
                    }
                }
            }


        }

    }
}
