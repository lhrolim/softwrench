﻿using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Metadata.Applications {
    public static class CompleteApplicationMetadataExtensions {

        /// <summary>
        /// retrieve a clone of the completemetadata, applying evental security restrictions associated for a given user
        /// </summary>
        /// <param name="originalMetadata"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static CompleteApplicationMetadataDefinition CloneSecuring(this CompleteApplicationMetadataDefinition originalMetadata,
           InMemoryUser user) {
            var schemas = originalMetadata.Schemas();
            var securedSchemas = new Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition>();

            foreach (var applicationSchema in schemas) {
                if (applicationSchema.Value.IsWebPlatform() || applicationSchema.Value.SchemaId.Equals(ApplicationMetadataConstants.SyncSchema)) {
                    continue;
                }
                var securedMetadata = originalMetadata.ApplyPolicies(applicationSchema.Key, user, ClientPlatform.Mobile, null);
                securedSchemas.Add(securedMetadata.Schema.GetSchemaKey(), securedMetadata.Schema);
            }

            return new CompleteApplicationMetadataDefinition(
                originalMetadata.Id,
                originalMetadata.ApplicationName,
                originalMetadata.Title,
                originalMetadata.Entity,
                originalMetadata.IdFieldName,
                originalMetadata.UserIdFieldName,
                originalMetadata.Parameters, securedSchemas, originalMetadata.DisplayableComponents,
                originalMetadata.Service,
                originalMetadata.Notifications);
        }

        public static bool IsWebSupported(this CompleteApplicationMetadataDefinition metadata) {
            return metadata.Schemas().Any(s => s.Key.Platform != ClientPlatform.Mobile);
        }

        public static bool IsMobileSupported(this CompleteApplicationMetadataDefinition metadata) {
            if (metadata.Parameters.ContainsKey(ApplicationMetadataConstants.MobileDisabled)) {
                return false;
            }

            return metadata.Schemas().Any(s => s.Key.Platform != ClientPlatform.Web);
        }

    }
}
