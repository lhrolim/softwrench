using cts.commons.portable.Util;
using cts.commons.Util;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Applications.Association;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using System;
using System.Linq;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications {
    public static class ApplicationMetadataExtensions {
        private const string NoSchemaFound = "could not find schema {0} in application {1}. Please review your metadata";

        [NotNull]
        public static ApplicationMetadata ApplyPolicies([NotNull] this CompleteApplicationMetadataDefinition application, ApplicationMetadataSchemaKey schemaKey, [NotNull] InMemoryUser user,
            ClientPlatform platform, string schemaFieldsToDisplay = null) {
            if (application == null) throw new ArgumentNullException("application");
            if (user == null) throw new ArgumentNullException("user");

            return new ApplicationMetadataPolicyApplier(application, schemaKey, user, platform, schemaFieldsToDisplay).Apply();
        }

        public static ApplicationMetadata ApplyPoliciesWeb([NotNull] this CompleteApplicationMetadataDefinition application,
            ApplicationMetadataSchemaKey schemaKey) {
            return new ApplicationMetadataPolicyApplier(application, schemaKey, SecurityFacade.CurrentUser(), ClientPlatform.Web, null).Apply();
        }

        [NotNull]
        public static ApplicationSchemaDefinition SchemaForPlatform([NotNull] this CompleteApplicationMetadataDefinition application, ApplicationMetadataSchemaKey metadataSchemaKey) {
            if (application == null) throw new ArgumentNullException("application");
            ApplicationSchemaDefinition resultingSchema;
            if (!application.Schemas().TryGetValue(metadataSchemaKey, out resultingSchema)) {
                if (metadataSchemaKey.SchemaId.EqualsAny(ApplicationMetadataConstants.List, ApplicationMetadataConstants.Detail)) {
                    //let´s give these default schema names a stereotype search fallback and return them case they are uniquely found
                    return SearchByStereotype(application, metadataSchemaKey, ref resultingSchema);
                } if (metadataSchemaKey.SchemaId.Equals(ApplicationMetadataConstants.SyncSchema)) {
                    //using list for now
                    var instance = application.SchemaForPlatform(new ApplicationMetadataSchemaKey(ApplicationMetadataConstants.List));
                    //                    var instance = ApplicationAssociationSchemaSyncFactory.GetInstance(application);
                    application.Schemas().Add(metadataSchemaKey, instance);
                    return instance;
                }
                throw new InvalidOperationException(String.Format(NoSchemaFound, metadataSchemaKey, application.ApplicationName));
            }
            return resultingSchema;
        }

        public static bool IsSupportedOnPlatform([NotNull] this CompleteApplicationMetadataDefinition application, ClientPlatform platform) {
            if (application == null) throw new ArgumentNullException("application");

            switch (platform) {
                case ClientPlatform.Web:
                    return application.IsWebSupported();

                case ClientPlatform.Mobile:
                    return application.IsMobileSupported();

                default:
                    throw new ArgumentOutOfRangeException(platform.ToString());
            }
        }

        public static ApplicationSchemaDefinition SearchByStereotype(CompleteApplicationMetadataDefinition application, ApplicationMetadataSchemaKey metadataSchemaKey, ref ApplicationSchemaDefinition resultSchema) {
            try {
                resultSchema = application.Schemas().Values.Single(schema => schema.Stereotype.ToString().EqualsIc(metadataSchemaKey.SchemaId));
            } catch (Exception) {
                // More than one schema found of the specified type
                throw new InvalidOperationException(String.Format(NoSchemaFound, metadataSchemaKey, application.ApplicationName));
            }
            return resultSchema;
        }
    }
}