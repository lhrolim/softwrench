using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Security {
    public static class InMemoryUserExtensions {



        private static bool IsAllowedOnApplication(this InMemoryUser user, CompleteApplicationMetadataDefinition application) {
            var appRoles = RoleManager.ActiveApplicationRoles();
            bool isAppRoleActive = appRoles.Contains(application.Role);
            return !isAppRoleActive || user.Roles.Any(r => r.Name == application.Role);
        }

        public enum SecurityModeCheckResult {
            Block, Allow, OutPut
        }


        public static SecurityModeCheckResult VerifySecurityMode(this InMemoryUser user, ApplicationMetadata application, DataRequestAdapter request) {
            if (user.IsSwAdmin()) {
                //SWDB apps have their own rule as for now.
                return SecurityModeCheckResult.Allow;
            }

            if (application.Name.StartsWith("_")) {
                return VerifySecurityModeSw(user, application);
            }

            var isTopLevelApp = MetadataProvider.FetchTopLevelApps(ClientPlatform.Web, null)
                .Any(a => a.ApplicationName.EqualsIc(application.Name));


            var profile = user.MergedUserProfile;
            var permission = profile.GetPermissionByApplication(application.Name, MetadataProvider.RoleByApplication(application.Name));
            if (permission == null) {
                if (isTopLevelApp) {
                    //no permission to that particular application
                    return SecurityModeCheckResult.Block;
                }
                return SecurityModeCheckResult.Allow;

            }
            var viewingExisting = request.Id != null || request.UserId != null;
            var isList = application.Schema.Stereotype == SchemaStereotype.List || request.SearchDTO != null;
            if (application.Schema.Stereotype.Equals(SchemaStereotype.Search)) {
                //TODO: think about this in the future
                return SecurityModeCheckResult.Allow;
            }

            if (isList && !permission.HasNoPermissions) {
                return SecurityModeCheckResult.Allow;
            }

            if (viewingExisting) {
                if (!permission.AllowUpdate && !permission.AllowView) {
                    return SecurityModeCheckResult.Block;
                } else if (!permission.AllowUpdate) {
                    //users can view, but using output mode only
                    return SecurityModeCheckResult.OutPut;
                }
            }
            if (!viewingExisting && !permission.AllowCreation) {
                return SecurityModeCheckResult.Block;
            }
            return SecurityModeCheckResult.Allow;
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

        public static ApplicationMetadata CachedSchema(this InMemoryUser user, String applicationName, ApplicationMetadataSchemaKey key) {

            if (!user.Genericproperties.ContainsKey("schemas")) {
                user.Genericproperties["schemas"] = new Dictionary<ApplicationMetadataSchemaKey, ApplicationMetadata>();
            }
            var cache = (IDictionary<ApplicationMetadataSchemaKey, ApplicationMetadata>)user.Genericproperties["schemas"];
            if (cache.ContainsKey(key)) {
                return cache[key];
            }
            var platform = key.Platform.HasValue ? key.Platform.Value : ClientPlatform.Web;
            var application = MetadataProvider.Application(applicationName).ApplyPolicies(key, user, platform);
            cache.Add(key, application);
            return application;

        }

        private static SecurityModeCheckResult VerifySecurityModeSw(this IPrincipal user, ApplicationMetadata application) {
            var applicationMetadata = MetadataProvider.Application(application.Name, false);
            if (applicationMetadata == null) {
                return SecurityModeCheckResult.Block;
            }

            var lookuper = SimpleInjectorGenericFactory.Instance.GetObject<IContextLookuper>();
            var context = lookuper.LookupContext();
            var local = ApplicationConfiguration.IsLocal();

            var isSysAdmin = user.IsInRole(Role.SysAdmin) || (local && context.MockSecurity);
            var sysAdminApplication = applicationMetadata.GetProperty(ApplicationSchemaPropertiesCatalog.SystemAdminApplication);
            if ("true".EqualsIc(sysAdminApplication) && isSysAdmin) {
                return SecurityModeCheckResult.Allow;
            }

            var isClientAdmin = user.IsInRole(Role.ClientAdmin) || (local && context.MockSecurity);
            var clientAdminApplication = applicationMetadata.GetProperty(ApplicationSchemaPropertiesCatalog.ClientAdminApplication);
            if ("true".EqualsIc(clientAdminApplication) && isClientAdmin) {
                return SecurityModeCheckResult.Allow;
            }

            return SecurityModeCheckResult.Block;
        }
    }
}
