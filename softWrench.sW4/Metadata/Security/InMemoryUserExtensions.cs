﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;

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
            var profile = user.MergedUserProfile;
            var permission = profile.GetPermissionByApplication(application.Name);
            if (permission == null) {
                //no restriction to that particular application
                return SecurityModeCheckResult.Allow;
            }
            var viewingExisting = request.Id != null || request.UserId != null;
            var isList = application.Schema.Stereotype == SchemaStereotype.List || request.SearchDTO != null;
            if (isList && permission.AllowView) {
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


    }
}
