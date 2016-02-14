using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Metadata.Applications {
    public static class ApplicationMetadataExtensions {

        private const string NoSchemaFound = "could not find schema {0} in application {1}. Please review your metadata";

        private const string MulitpleStereotypesDeclared = "Multiple stereotypes {0} were defined for application {1} could not determine default route. Please review your metadata";

        [NotNull]
        public static ApplicationMetadata ApplyPolicies([NotNull] this CompleteApplicationMetadataDefinition application, ApplicationMetadataSchemaKey schemaKey, [NotNull] InMemoryUser user,
            ClientPlatform platform, string schemaFieldsToDisplay = null) {
            if (application == null) throw new ArgumentNullException("application");
            if (user == null) throw new ArgumentNullException("user");

            return new ApplicationMetadataPolicyApplier(application, schemaKey, user, platform, schemaFieldsToDisplay).Apply();
        }

        public static ApplicationMetadata ApplyPoliciesWeb([NotNull] this CompleteApplicationMetadataDefinition application,
            ApplicationMetadataSchemaKey schemaKey) {
            if (schemaKey != null) {
                schemaKey.Platform = ClientPlatform.Web;
            }
            return new ApplicationMetadataPolicyApplier(application, schemaKey, SecurityFacade.CurrentUser(), ClientPlatform.Web, null).Apply();
        }

        [NotNull]
        public static ApplicationSchemaDefinition SchemaForPlatform([NotNull] this CompleteApplicationMetadataDefinition application, ApplicationMetadataSchemaKey metadataSchemaKey) {
            if (application == null) throw new ArgumentNullException("application");
            ApplicationSchemaDefinition resultingSchema;
            if (!application.Schemas().TryGetValue(metadataSchemaKey, out resultingSchema)) {
                var schemaId = metadataSchemaKey.SchemaId;
                if (schemaId.EqualsAny(ApplicationMetadataConstants.List, ApplicationMetadataConstants.Detail)) {
                    //let´s give these default schema names a stereotype search fallback and return them case they are uniquely found
                    resultingSchema = SchemaByStereotype(application, schemaId);
                    return resultingSchema;
                }
                if (schemaId.Equals(ApplicationMetadataConstants.SyncSchema)) {
                    //using list for now
                    var instance = application.SchemaForPlatform(new ApplicationMetadataSchemaKey(ApplicationMetadataConstants.List));
                    //                    var instance = ApplicationAssociationSchemaSyncFactory.GetInstance(application);
                    application.Schemas().Add(metadataSchemaKey, instance);
                    return instance;
                }
                throw new InvalidOperationException(string.Format(NoSchemaFound, metadataSchemaKey, application.ApplicationName));
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

        [CanBeNull]
        public static ApplicationSchemaDefinition SchemaByStereotype(this CompleteApplicationMetadataDefinition application, string stereotypeName, bool throwException = false) {
            try {
                return application.Schemas().Values.Single(schema => (schema.Stereotype.ToString().EqualsIc(stereotypeName) && !schema.Abstract));
            } catch (Exception) {
                if (throwException) {
                    // More than one schema found of the specified type
                    throw new InvalidOperationException(string.Format(MulitpleStereotypesDeclared, stereotypeName, application.ApplicationName));
                }
                return null;
            }
        }


        public static IEnumerable<ApplicationSchemaDefinition> NonInternalSchemasByStereotype(this CompleteApplicationMetadataDefinition application, string stereotypeName, ClientPlatform platform = ClientPlatform.Web) {
            var schemas = MetadataProvider.FetchNonInternalSchemas(platform, application.ApplicationName);
            if (stereotypeName == "detail") {
                return schemas.Where(schema => (schema.StereotypeAttr.ToLower().StartsWith(stereotypeName) && schema.StereotypeAttr.ToLower() != "detailnew" && !schema.Abstract));
            }

            return schemas.Where(schema => (schema.StereotypeAttr.ToLower().StartsWith(stereotypeName) && !schema.Abstract));
        }


        public static ApplicationSchemaDefinition PreferredSchemaByStereotype(this CompleteApplicationMetadataDefinition application, string stereotypeName) {


            var schemas = application.Schemas().Values.Where(schema => (schema.StereotypeAttr.ToLower().StartsWith(stereotypeName) && !schema.Abstract));

            var applicationSchemaDefinitions = schemas as ApplicationSchemaDefinition[] ?? schemas.ToArray();
            if (applicationSchemaDefinitions.Count() > 1) {
                return null;
            }
            return applicationSchemaDefinitions.FirstOrDefault();
        }
    }
}