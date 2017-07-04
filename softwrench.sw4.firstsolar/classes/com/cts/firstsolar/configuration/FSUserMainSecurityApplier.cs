using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Security;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration {


    [OverridingComponent(ClientFilters = "firstsolar")]
    public class FSUserMainSecurityApplier : UserMainSecurityApplier {

        public override InMemoryUserExtensions.SecurityModeCheckResult VerifyMainSecurityMode(InMemoryUser user, ApplicationMetadata application,
            DataRequestAdapter request){
            var securityResult = base.VerifyMainSecurityMode(user, application, request);
            var profile = user.MergedUserProfile;
            var wpPermission = profile.GetPermissionByApplication("_workpackage");
            if (wpPermission == null || wpPermission.HasNoPermissions) {
                return securityResult;
            }

            if (application.Name.EqualsIc("workorder")) {
                //if the user has access to workpackage, it should be given access to workorders schemas as well, otherwise some lookups would fail
                return InMemoryUserExtensions.SecurityModeCheckResult.Allow;
            }
            return securityResult;
        }
    }
}
