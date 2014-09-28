using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;

namespace softWrench.sW4.Metadata.Security {
    public static class InMemoryUserExtensions {



        private static bool IsAllowedOnApplication(this InMemoryUser user, CompleteApplicationMetadataDefinition application) {
            var appRoles = RoleManager.ActiveApplicationRoles();
            bool isAppRoleActive = appRoles.Contains(application.Role);
            return !isAppRoleActive || user.Roles.Any(r => r.Name == application.Role);
        }

        [NotNull]
        public static IEnumerable<CompleteApplicationMetadataDefinition> Applications([NotNull] this InMemoryUser user) {
            if (user == null) throw new ArgumentNullException("user");

            return from application in MetadataProvider.Applications()
                   where user.IsAllowedOnApplication(application)
                   select application;
        }

        [NotNull]
        public static IEnumerable<CompleteApplicationMetadataDefinition> Applications([NotNull] this InMemoryUser user, ClientPlatform platform) {
            if (user == null) throw new ArgumentNullException("user");

            return from application in MetadataProvider.Applications(platform)
                   where user.IsAllowedOnApplication(application)
                   select application;

        }

     
    }
}
