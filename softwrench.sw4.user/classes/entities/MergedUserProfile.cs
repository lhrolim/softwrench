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
        IList<ApplicationPermission> _permissions;

        public MergedUserProfile()
        {
            FieldPermission fp = new FieldPermission();
            fp.FieldKey = "owner";
            fp.Permission = "readonly";
            fp.Id = 1;
            ContainerPermission cp = new ContainerPermission();
            cp.Schema = "editdetail";
            cp.ContainerKey = "main";
            cp.Id = 1;
            List<FieldPermission> fps = new List<FieldPermission>();
            fps.Add(fp);
            cp.FielsPermissions = new HashedSet<FieldPermission>(fps);
            ApplicationPermission ap = new ApplicationPermission();
            ap.ApplicationName = "servicerequest";
            ap.Id = 1;
            ap.AllowCreation = true;
            ap.AllowRemoval = true;
            ap.AllowUpdate = true;
            ap.AllowViewOnly = false;
            List<ContainerPermission> cps = new List<ContainerPermission>();
            cps.Add(cp);
            ap.ContainerPermissions = new HashedSet<ContainerPermission>(cps);
            _permissions = new List<ApplicationPermission>();
            _permissions.Add(ap);
        }

        public ApplicationPermission GetPermissionByApplication(string applicationName) {
            if (!_permissions.Any(p => p.ApplicationName.ToLower().EqualsIc(applicationName))) {
                return null;
            }
            var result = _permissions.Single(p => p.ApplicationName.ToLower() == applicationName.ToLower());
            return result;
        }

        public IEnumerable<ContainerPermission> GetPermissionBySchema(string applicationName, string schemaId) {
            var applicationPermissions = GetPermissionByApplication(applicationName);
            var containerPermissions = applicationPermissions.ContainerPermissions.Where(c => c.Schema.EqualsIc(schemaId));
            return containerPermissions;
        }
    }
}
