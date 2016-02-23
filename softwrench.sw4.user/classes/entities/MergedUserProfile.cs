using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.user.classes.entities.security;

namespace softwrench.sw4.user.classes.entities {
    public class MergedUserProfile {
        public IEnumerable<ApplicationPermission> Permissions {
            get; set;
        }

        public IEnumerable<Role> Roles {
            get; set;
        }

        public MergedUserProfile() {
            Permissions = new List<ApplicationPermission>();
            Roles = new List<Role>();
        }

        public ApplicationPermission GetPermissionByApplication(string applicationName) {
            var result = Permissions.Single(p => p.ApplicationName.ToLower() == applicationName.ToLower());
            return result;
        }

        public IEnumerable<ContainerPermission> GetPermissionBySchema(string applicationName, string schemaId) {
            var applicationPermission = GetPermissionByApplication(applicationName);
            var schemaPermission = applicationPermission.ContainerPermissions.Where(s => s.Schema.ToLower() == schemaId.ToLower());
            return schemaPermission;
        }
    }
}
