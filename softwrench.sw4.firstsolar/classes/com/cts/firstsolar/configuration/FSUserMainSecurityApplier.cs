using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Security;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration {


    [OverridingComponent(ClientFilters = "firstsolar")]
    public class FSUserMainSecurityApplier : UserMainSecurityApplier {

        protected override ApplicationPermission LookupPermission(ApplicationMetadata application, MergedUserProfile profile) {
            var basePermission = base.LookupPermission(application, profile);
            var wpPermission = profile.GetPermissionByApplication("_workpackage");
            if (wpPermission == null || wpPermission.HasNoPermissions) {
                return basePermission;
            }

            if (application.Name.EqualsIc("workorder")) {
                //if the user has access to workpackage, it should be given access to workorders schemas as well, otherwise some lookups would fail
                basePermission.AllowUpdate = true;
            }
            return basePermission;

        }
    }
}
