using NHibernate.Util;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Metadata;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using log4net;
using softWrench.sW4.Metadata.Applications.Security;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Data.Relationship.Composition {

    //TODO: should clean cache upon metadata saving
    public class CompositionBuilder : ISingletonComponent {



        public static IDictionary<string, ApplicationCompositionSchema> InitializeCompositionSchemas(ApplicationSchemaDefinition schema, InMemoryUser user = null) {
            if (schema.CachedCompositions != null) {
                return ApplicationCompositionSecurityApplier.ApplySecurity(schema, schema.CachedCompositions, user);
            }
            var compositionMetadatas = new Dictionary<string, ApplicationCompositionSchema>();
            foreach (var composition in schema.Compositions()) {
                compositionMetadatas.Add(composition.Relationship, DoInitializeCompositionSchemas(schema, composition, new IdentitySet()));
            }
            schema.CachedCompositions = compositionMetadatas;
            return ApplicationCompositionSecurityApplier.ApplySecurity(schema, schema.CachedCompositions, user);
        }



        private static ApplicationCompositionSchema DoInitializeCompositionSchemas(ApplicationSchemaDefinition schema, ApplicationCompositionDefinition composition, IdentitySet checkedCompositions) {
            var relationship = composition.Relationship;
            var compositionApplication = MetadataProvider.GetCompositionApplication(schema, relationship);
            var compositionResult = new CompositionSchemas();
            var applicationCompositionSchema = composition.Schema;
            if (applicationCompositionSchema.Schemas != null) {
                return applicationCompositionSchema;
            }
            compositionResult.Detail = GetDetailSchema(compositionApplication, applicationCompositionSchema);
            if (composition.Collection && applicationCompositionSchema is ApplicationCompositionCollectionSchema) {
                compositionResult.List = GetListSchema(applicationCompositionSchema, compositionApplication);
                compositionResult.Print = GetPrintSchema(applicationCompositionSchema, compositionApplication);
                if (!composition.IsSelfRelationship) {
                    //this won´t be needed into the synchronization
                    compositionResult.Sync = GetSyncSchema(applicationCompositionSchema, compositionApplication);
                }
                var collSchema = (ApplicationCompositionCollectionSchema)applicationCompositionSchema;
                collSchema.FetchFromServer = ShouldFetchFromServer(compositionResult.Detail, compositionResult.List);
            }
            //build eventual inner compositions schema data
            checkedCompositions.Add(composition.Relationship);
            if (compositionResult.Detail != null) {
                //sometimes,we don´t have a detail schema, just a list one
                foreach (var innerComposition in compositionResult.Detail.Compositions()) {
                    if (!checkedCompositions.Contains(innerComposition.Relationship)) {
                        //to avod infinte loop
                        DoInitializeCompositionSchemas(schema, innerComposition, checkedCompositions);
                    }
                }
            }
            applicationCompositionSchema.Schemas = compositionResult;
            return applicationCompositionSchema;
        }

        private static ApplicationSchemaDefinition GetSyncSchema(ApplicationCompositionSchema compositionSchema,
            CompleteApplicationMetadataDefinition compositionApplication) {
            if (!MetadataProvider.isMobileEnabled()) {
                return null;
            }


            var applicationSchemaDefinitions = compositionApplication.Schemas();
            var syncKey = ApplicationMetadataSchemaKey.GetSyncInstance();
            if (applicationSchemaDefinitions.ContainsKey(syncKey)) {
                return applicationSchemaDefinitions[syncKey];
            }
            return GetDetailSchema(compositionApplication, compositionSchema);
        }

        /// <summary>
        /// If there´s a schema named print, use it, otherwise fallback to detail
        /// </summary>
        /// <param name="compositionSchema"></param>
        /// <param name="compositionApplication"></param>
        /// <returns></returns>
        private static ApplicationSchemaDefinition GetPrintSchema(ApplicationCompositionSchema compositionSchema,
            CompleteApplicationMetadataDefinition compositionApplication) {
            var applicationSchemaDefinitions = compositionApplication.Schemas();
            var printKey = new ApplicationMetadataSchemaKey(compositionSchema.PrintSchema, compositionSchema.RenderMode, ClientPlatform.Web);
            if (applicationSchemaDefinitions.ContainsKey(printKey)) {
                return applicationSchemaDefinitions[printKey];
            }
            return GetDetailSchema(compositionApplication, compositionSchema);
        }

        private static bool ShouldFetchFromServer(ApplicationSchemaDefinition detail, ApplicationSchemaDefinition list) {
            if (detail == null) {
                //sometimes we might need only to see the list
                return false;
            }
            if (EnumerableExtensions.Any(detail.Associations()) || EnumerableExtensions.Any(detail.Compositions())) {
                return true;
            }
            foreach (var displayable in detail.Displayables) {
                if (!(displayable is IApplicationAttributeDisplayable)) {
                    return true;
                }
                var attrDisplayable = (IApplicationAttributeDisplayable)displayable;
                if (list.Fields.All(f => f.Attribute != attrDisplayable.Attribute)) {
                    return true;
                }
            }

            //return true, if detail has more data then list
            return false;
        }

        private static ApplicationSchemaDefinition GetListSchema(ApplicationCompositionSchema applicationCompositionSchema, CompleteApplicationMetadataDefinition compositionApplication) {
            var collectionSchema = (ApplicationCompositionCollectionSchema)applicationCompositionSchema;
            var listKey = new ApplicationMetadataSchemaKey(collectionSchema.CollectionProperties.ListSchema,
                applicationCompositionSchema.RenderMode, ClientPlatform.Web);
            return compositionApplication.Schemas()[listKey];
        }

        private static ApplicationSchemaDefinition GetDetailSchema(CompleteApplicationMetadataDefinition compositionApplication,
            ApplicationCompositionSchema compositionSchema) {
            if (compositionSchema.DetailSchema == "") {
                //This means that the composition is only needed for list visualization
                return null;
            }
            var detailKey = new ApplicationMetadataSchemaKey(compositionSchema.DetailSchema, compositionSchema.RenderMode, ClientPlatform.Web);
            var applicationSchemaDefinitions = compositionApplication.Schemas();
            if (!applicationSchemaDefinitions.ContainsKey(detailKey)) {
                throw ExceptionUtil.MetadataException(
                    "detail composition schema {0} not found for application {1}. Use \"\" if you want to specify that this is used only for list", detailKey, compositionApplication.ApplicationName);
            }
            return applicationSchemaDefinitions[detailKey];
        }
    }
}
