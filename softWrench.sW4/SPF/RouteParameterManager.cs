using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sw4.Shared2.Util;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Command;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Metadata.Stereotypes.Schema;

namespace softWrench.sW4.SPF {
    public class RouteParameterManager {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentApplication">The current resolved schema the user is</param>
        /// <param name="routerDTO">Eventual parameters coming from client side</param>
        /// <param name="platform">to help to resolve the next schema</param>
        /// <param name="user">current logged user,needed to apply eventual role policies to the schema found</param>
        /// <param name="operation"></param>
        /// <param name="resolvedNextSchemaKey">deprecated parameter only used for operations, need to migrate this</param>
        /// <returns>The resolved application metadata for the next schema, considering role policies of the user. If there´s no redirection it shall return the same schema</returns>
        [NotNull]
        public static ApplicationMetadata FillNextSchema([NotNull]ApplicationMetadata currentApplication, [NotNull]RouterParametersDTO routerDTO, ClientPlatform platform, [NotNull]InMemoryUser user, [CanBeNull]string operation,
            [CanBeNull]ApplicationMetadataSchemaKey resolvedNextSchemaKey = null) {

            var nextSchema = resolvedNextSchemaKey;
            if (nextSchema == null) {
                //legacy support for operations... they are still coming inside the json
                //TODO: remove
                var nextSchemaKey = routerDTO.NextSchemaKey;
                if (nextSchemaKey == null) {

                    var applicationCommand = ApplicationCommandUtils.GetApplicationCommand(currentApplication, operation);
                    if (applicationCommand != null && !string.IsNullOrWhiteSpace(applicationCommand.NextSchemaId)) {
                        nextSchemaKey = applicationCommand.NextSchemaId;
                    } else {
                        //if not specified from the client, let´s search on the schema definition
                        nextSchemaKey =
                            currentApplication.Schema.GetProperty(ApplicationSchemaPropertiesCatalog.RoutingNextSchemaId);
                    }
                }
                if (nextSchemaKey != null) {
                    nextSchema = SchemaUtil.GetSchemaKeyFromString(nextSchemaKey, platform);
                } else {
                    if (SchemaStereotype.DetailNew.Equals(currentApplication.Schema.Stereotype)) {
                        //https://controltechnologysolutions.atlassian.net/browse/SWWEB-1640
                        var completeCurrentApp = MetadataProvider.Application(currentApplication.Name);
                        var detailSchema = completeCurrentApp.SchemaByStereotype(SchemaStereotype.Detail.ToString());
                        if (detailSchema != null) {
                            return completeCurrentApp.ApplyPolicies(detailSchema.GetSchemaKey(), user, platform);
                        }
                    }

                    //if it was still not specified in any place, stay on the same schema
                    return currentApplication;
                }
            }


            var nextApplicationName = routerDTO.NextApplicationName;
            if (nextApplicationName == null) {
                nextApplicationName =
                    currentApplication.Schema.GetProperty(ApplicationSchemaPropertiesCatalog.RoutingNextApplication);
                if (nextApplicationName == null) {
                    //use the same application as current by default,since it´s rare to switch applications
                    nextApplicationName = currentApplication.Name;
                }
            }
            var nextApplication = MetadataProvider.Application(nextApplicationName);
            return nextApplication.ApplyPolicies(nextSchema, user, platform);

        }
    }
}
