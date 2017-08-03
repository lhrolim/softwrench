using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.application;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.Security {

    public class UserMainSecurityApplier : ISingletonComponent {


        [Import]
        public IContextLookuper ContextLookuper { get; set; }


        public virtual InMemoryUserExtensions.SecurityModeCheckResult VerifyMainSecurityMode(InMemoryUser user, ApplicationMetadata application, DataRequestAdapter request) {
            if (user.IsSwAdmin()) {
                //SWDB apps have their own rule as for now.
                return InMemoryUserExtensions.SecurityModeCheckResult.Allow;
            }

            var isTopLevelApp = MetadataProvider.FetchTopLevelApps(ClientPlatform.Web, null)
                .Any(a => a.ApplicationName.EqualsIc(application.Name));

            if (application.Name.StartsWith("_") && !isTopLevelApp) {
                return VerifySecurityModeSw(user, application);
            }

            var profile = user.MergedUserProfile;
            var permission = LookupPermission(application, profile);

            if (permission == null) {
                if (isTopLevelApp) {
                    //no permission to that particular application
                    return InMemoryUserExtensions.SecurityModeCheckResult.Block;
                }
                return InMemoryUserExtensions.SecurityModeCheckResult.Allow;

            }
            var viewingExisting = request.Id != null || request.UserId != null;
            var isList = application.Schema.Stereotype == SchemaStereotype.List || request.SearchDTO != null;
            if (application.Schema.Stereotype.Equals(SchemaStereotype.Search)) {
                //TODO: think about this in the future
                return InMemoryUserExtensions.SecurityModeCheckResult.Allow;
            }

            if (isList && !permission.HasNoPermissions) {
                return InMemoryUserExtensions.SecurityModeCheckResult.Allow;
            }

            if (viewingExisting) {
                if (permission.HasNoPermissions) {
                    return InMemoryUserExtensions.SecurityModeCheckResult.Block;
                }
                if (!permission.AllowUpdate) {
                    //users can view, but using output mode only
                    return InMemoryUserExtensions.SecurityModeCheckResult.OutPut;
                }
            }
            if (!viewingExisting && !permission.AllowCreation) {
                return InMemoryUserExtensions.SecurityModeCheckResult.Block;
            }
            return InMemoryUserExtensions.SecurityModeCheckResult.Allow;
        }

        protected virtual ApplicationPermission LookupPermission(ApplicationMetadata application, MergedUserProfile profile) {
            return profile.GetPermissionByApplication(application.Name, MetadataProvider.RoleByApplication(application.Name));
        }

        private InMemoryUserExtensions.SecurityModeCheckResult VerifySecurityModeSw(IPrincipal user, ApplicationMetadata application) {
            var applicationMetadata = MetadataProvider.Application(application.Name, false);
            if (applicationMetadata == null) {
                return InMemoryUserExtensions.SecurityModeCheckResult.Block;
            }


            var context = ContextLookuper.LookupContext();
            var local = ApplicationConfiguration.IsLocal();

            var isSysAdmin = user.IsInRole(Role.SysAdmin) || (local && context.MockSecurity);
            var sysAdminApplication = applicationMetadata.GetProperty(ApplicationSchemaPropertiesCatalog.SystemAdminApplication);
            if ("true".EqualsIc(sysAdminApplication) && isSysAdmin) {
                return InMemoryUserExtensions.SecurityModeCheckResult.Allow;
            }

            var isClientAdmin = user.IsInRole(Role.ClientAdmin) || (local && context.MockSecurity);
            var clientAdminApplication = applicationMetadata.GetProperty(ApplicationSchemaPropertiesCatalog.ClientAdminApplication);
            if ("true".EqualsIc(clientAdminApplication) && isClientAdmin) {
                return InMemoryUserExtensions.SecurityModeCheckResult.Allow;
            }

            return InMemoryUserExtensions.SecurityModeCheckResult.Block;
        }

    }
}
