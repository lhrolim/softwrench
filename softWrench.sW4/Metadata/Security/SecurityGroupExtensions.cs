﻿using System.Linq;
using cts.commons.portable.Util;
using softwrench.sw4.user.classes.entities;

namespace softWrench.sW4.Metadata.Security {
    public static class SecurityGroupExtensions {

        public static bool HasApplicationPermission(this UserProfile profile, string applicationName) {
            return profile.Roles.Any(r => r.Name.EqualsIc(MetadataProvider.RoleByApplication(applicationName)));
        }

    }
}