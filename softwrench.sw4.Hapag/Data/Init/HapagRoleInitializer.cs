using System.Collections;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace softwrench.sw4.Hapag.Data.Init {
    class HapagRoleInitializer {
        private readonly SWDBHibernateDAO _dao;

        public HapagRoleInitializer(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        public RoleSaveResult SaveRoles() {

            var roles = SaveRoles<RoleType>(typeof(RoleType));
            var functionalRoles = SaveRoles<FunctionalRole>(typeof(FunctionalRole));

            return new RoleSaveResult {
                FunctionalRoles = functionalRoles,
                OrdinaryRoles = roles
            };

        }

        //          public T Save<T>(T ob) where T : class {

        private IDictionary<T, Role> SaveRoles<T>(Type type) {
            var roleTypes = Enum.GetValues(type).Cast<T>();
            IDictionary<T, Role> result = new Dictionary<T, Role>();
            foreach (var r in roleTypes) {
                var roleType = r as Enum;
                var roleName = roleType.GetName();
                var role = _dao.FindSingleByQuery<Role>(Role.RoleByName, roleName);
                if (role != null) {
                    if (role.Deletable) {
                        role.Deletable = false;
                        _dao.Save(role);
                    }
                    result.Add(r, role);
                    continue;
                }
                role = new Role { Name = roleName, Active = true, Deletable = false };
                role = _dao.Save(role);
                result.Add(r, role);
            }
            return result;
        }




        internal class RoleSaveResult {
            internal IDictionary<RoleType, Role> OrdinaryRoles = new Dictionary<RoleType, Role>();
            internal IDictionary<FunctionalRole, Role> FunctionalRoles = new Dictionary<FunctionalRole, Role>();

        }

    }



}
