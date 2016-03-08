using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using log4net;
using log4net.Core;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softWrench.sW4.Metadata.Entities;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata.Applications.Schema;

namespace softWrench.sW4.Metadata.Validator {
    class MetadataMerger {

        private static ILog Log = LogManager.GetLogger(typeof(MetadataMerger));

        public MetadataMerger() {
            Log.Debug("init log...");
        }

        private const string CustomizeAndRedeclare = "schemas using customizations must have redeclaring=false";



        public static IEnumerable<TR> Merge<TR>(IEnumerable<TR> sourceItems, IEnumerable<TR> overridenItems) {
            var enumerable = overridenItems as IList<TR> ?? overridenItems.ToList();
            if (!enumerable.Any()) {
                return sourceItems;
            }

            if (enumerable.First() is CompleteApplicationMetadataDefinition) {
                return (IEnumerable<TR>)MergeApplications((IEnumerable<CompleteApplicationMetadataDefinition>)sourceItems, (IEnumerable<CompleteApplicationMetadataDefinition>)enumerable);
            }
            return (IEnumerable<TR>)MergeEntities((IEnumerable<EntityMetadata>)sourceItems, (IEnumerable<EntityMetadata>)enumerable);
        }


        public static IEnumerable<CompleteApplicationMetadataDefinition> MergeApplications(IEnumerable<CompleteApplicationMetadataDefinition> sourceApplications,
            IEnumerable<CompleteApplicationMetadataDefinition> overridenApplications) {
            IList<CompleteApplicationMetadataDefinition> resultApplications = new List<CompleteApplicationMetadataDefinition>();
            var completeApplicationMetadataDefinitions = overridenApplications as CompleteApplicationMetadataDefinition[] ?? overridenApplications.ToArray();
            foreach (var souceAplication in sourceApplications) {
                var overridenApplication = completeApplicationMetadataDefinitions.FirstOrDefault(a => a.ApplicationName.EqualsIc(souceAplication.ApplicationName));
                if (overridenApplication != null) {
                    resultApplications.Add(DoMergeApplication(souceAplication, overridenApplication));
                } else {
                    resultApplications.Add(souceAplication);
                }
            }
            foreach (var overridenApplication in completeApplicationMetadataDefinitions) {
                if (resultApplications.All(f => f.ApplicationName != overridenApplication.ApplicationName)) {
                    resultApplications.Add(overridenApplication);
                }
            }

            return resultApplications;
        }

        private static CompleteApplicationMetadataDefinition DoMergeApplication([NotNull]CompleteApplicationMetadataDefinition souceAplication, [NotNull]CompleteApplicationMetadataDefinition overridenApplication) {
            IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> resultSchemas = new Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition>();
            var resultComponents = MergeComponents(souceAplication, overridenApplication);
            var resultFilters = SchemaFilterBuilder.ApplyFilterCustomizations(souceAplication.AppFilters, overridenApplication.AppFilters);
            Log.InfoFormat("applying merge for application {0}".Fmt(overridenApplication.ApplicationName));

            foreach (var schema in souceAplication.Schemas()) {

                ApplicationSchemaDefinition overridenSchema;
                overridenApplication.Schemas().TryGetValue(schema.Key, out overridenSchema);

                if (schema.Value.Stereotype.Equals(SchemaStereotype.List)) {
                    schema.Value.DeclaredFilters.Merge(resultFilters);
                }

                if (overridenSchema == null) {
                    //this is for adding the base schemas that have no redeclaration (i.e. they exist only on the templates)
                    resultSchemas.Add(schema.Key, schema.Value);
                } else {

                    if (overridenSchema.Stereotype.Equals(SchemaStereotype.List)) {
                        overridenSchema.DeclaredFilters.Merge(resultFilters);
                        if (overridenSchema.FiltersResolved) {
                            //second pass
                            overridenSchema.SchemaFilters = resultFilters;
                        }
                    }

                    if (!overridenSchema.RedeclaringSchema) {
                        //if we´re not redeclaring, then we need to first add the original one and merge the customizations on top of it
                        resultSchemas.Add(schema.Key, schema.Value);
                        SchemaMerger.MergeSchemas(schema.Value, overridenSchema, resultComponents);
                    } else {
                        if (SchemaMerger.IsCustomized(overridenSchema)) {
                            throw new MetadataException(CustomizeAndRedeclare);
                        }
                        //if redeclaring though, we need to ignore the old one and just insert the new one
                        resultSchemas.Add(schema.Key, overridenSchema);
                    }
                }
            }

            foreach (var overridenSchema in overridenApplication.Schemas()) {
                if (souceAplication.Schemas().All(f => !f.Key.Equals(overridenSchema.Key))) {
                    //adding any schemas that are only declared on the overriden application (new schemas that are not present on templates...)
                    resultSchemas.Add(overridenSchema.Key, overridenSchema.Value);
                }
            }

            var overridenParameters = souceAplication.MergeProperties(overridenApplication);


            var title = overridenApplication.Title ?? souceAplication.Title;
            var entity = overridenApplication.Entity ?? souceAplication.Entity;
            var idFieldName = overridenApplication.IdFieldName ?? souceAplication.IdFieldName;
            var userIdFieldName = overridenApplication.UserIdFieldName ?? souceAplication.UserIdFieldName;
            var service = overridenApplication.Service ?? souceAplication.Service;
            var role = overridenApplication.Role ?? souceAplication.Role;
            var auditEnabled = overridenApplication.AuditFlag ?? souceAplication.AuditFlag;
            var mergedFilters = SchemaFilterBuilder.ApplyFilterCustomizations(souceAplication.AppFilters, overridenApplication.AppFilters);

            return new CompleteApplicationMetadataDefinition(souceAplication.Id, souceAplication.ApplicationName,
                title, entity, idFieldName, userIdFieldName,
                overridenParameters, resultSchemas, souceAplication.DisplayableComponents.Union(overridenApplication.DisplayableComponents), mergedFilters, service, role, auditEnabled);

        }



        private static List<DisplayableComponent> MergeComponents(CompleteApplicationMetadataDefinition souceAplication,
            CompleteApplicationMetadataDefinition overridenApplication) {
            var resultComponents = new List<DisplayableComponent>();
            foreach (var component in souceAplication.DisplayableComponents) {
                var overridenComponent = overridenApplication.DisplayableComponents.FirstOrDefault(f => f.Id == component.Id);
                if (overridenComponent == null) {
                    resultComponents.Add(component);
                } else {
                    resultComponents.Add(overridenComponent);
                }
            }
            foreach (var overridenComponent in overridenApplication.DisplayableComponents) {
                if (resultComponents.All(f => f.Id != overridenComponent.Id)) {
                    resultComponents.Add(overridenComponent);
                }
            }

            return resultComponents;
        }


        public static IEnumerable<EntityMetadata> MergeEntities(IEnumerable<EntityMetadata> sourceEntities, IEnumerable<EntityMetadata> overridenEntities) {
            IList<EntityMetadata> resultEntities = new List<EntityMetadata>();
            var entityMetadatas = overridenEntities as EntityMetadata[] ?? overridenEntities.ToArray();
            foreach (var sourceEntity in sourceEntities) {
                var overridenEntity = entityMetadatas.FirstOrDefault(a => a.Name.EqualsIc(sourceEntity.Name));
                if (overridenEntity != null) {
                    DoMergeEntity(sourceEntity, overridenEntity);
                }
                resultEntities.Add(sourceEntity);
            }
            foreach (var overridenEntity in overridenEntities) {
                if (resultEntities.All(f => f.Name != overridenEntity.Name)) {
                    resultEntities.Add(overridenEntity);
                }
            }

            return resultEntities;
        }

        // TODO: When merging entities, the idAttribute and useridAttribute on the entity are not being merged. Parent values are retained and overriden values are ignored.
        private static void DoMergeEntity(EntityMetadata sourceEntity, EntityMetadata overridenEntity) {
            foreach (var association in overridenEntity.Associations) {
                if (overridenEntity.Schema.ExcludeUndeclaredAssociations) {
                    sourceEntity.Associations.Clear();
                }
                if (sourceEntity.Associations.Contains(association)) {
                    sourceEntity.Associations.Remove(association);
                }
                sourceEntity.Associations.Add(association);
            }
            foreach (var attribute in overridenEntity.Schema.Attributes) {
                if (overridenEntity.Schema.ExcludeUndeclaredAttributes) {
                    sourceEntity.Schema.Attributes.Clear();
                }
                if (sourceEntity.Schema.Attributes.Contains(attribute)) {
                    sourceEntity.Schema.Attributes.Remove(attribute);
                }
                sourceEntity.Schema.Attributes.Add(attribute);
            }

            foreach (var parameter in overridenEntity.ConnectorParameters.Parameters) {
                if (overridenEntity.ConnectorParameters.ExcludeUndeclared) {
                    sourceEntity.ConnectorParameters.Parameters.Clear();
                }
                if (sourceEntity.ConnectorParameters.Parameters.ContainsKey(parameter.Key)) {
                    sourceEntity.ConnectorParameters.Parameters.Remove(parameter.Key);
                }
                sourceEntity.ConnectorParameters.Parameters.Add(parameter);
            }
            if (overridenEntity.HasWhereClause) {
                sourceEntity.WhereClause = overridenEntity.WhereClause;
            }
        }


    }
}
