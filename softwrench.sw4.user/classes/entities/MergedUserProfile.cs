using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Iesi.Collections.Generic;
using softwrench.sw4.user.classes.entities.security;

namespace softwrench.sw4.user.classes.entities
{
    public class MergedUserProfile
    {
        public IEnumerable<ApplicationPermission> Permissions {
            get; set;
        }

        public MergedUserProfile()
        {
            Permissions = new List<ApplicationPermission>();
            Roles = new List<Role>();
        }

        public IEnumerable<Role> Roles {
            get; set;
        }

        public ApplicationPermission GetPermissionByApplication(string applicationName) {
            return Permissions.FirstOrDefault(p => p.ApplicationName.EqualsIc(applicationName));
        }

        public IEnumerable<ContainerPermission> GetPermissionBySchema(string applicationName, string schemaId) {
            var applicationPermissions = GetPermissionByApplication(applicationName);
            var containerPermissions = applicationPermissions.ContainerPermissions.Where(c => c.Schema.EqualsIc(schemaId));
            return containerPermissions;
        }
    }
}
