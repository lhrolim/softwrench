using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Security;
using softWrench.sW4.Metadata.Menu;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration {


    [OverridingComponent(ClientFilters = "firstsolar")]
    public class FSMenuSecurityManager : MenuSecurityManager {

        protected override bool IsApplicationMenuSecured(ApplicationMenuItemDefinition leaf, MergedUserProfile mergedUserProfile, ClientPlatform platform) {
            if (leaf.Application.EqualsIc("workorder") && leaf.Schema.EqualsIc("wplist")) {
                var application = mergedUserProfile.GetPermissionByApplication("_WorkPackage");
                if (application == null) {
                    //not allowed by default, no permission rule
                    return false;
                }
                if (application.HasNoPermissions) {
                    return false;
                }
                return true;
            }

            return base.IsApplicationMenuSecured(leaf, mergedUserProfile, platform);
        }
    }
}
