using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NHibernate.Util;
using softWrench.sW4.Metadata.Entities;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {
    class MetadataMerger {

        public static IEnumerable<TR> Merge<TR>(IEnumerable<TR> sourceItems, IEnumerable<TR> overridenItems) {
            var enumerable = overridenItems as IList<TR> ?? overridenItems.ToList();
            if (!enumerable.Any()) {
                return sourceItems;
            }

            if (enumerable.First() is CompleteApplicationMetadataDefinition) {
                return (IEnumerable<TR>)MergeApplications((IEnumerable<CompleteApplicationMetadataDefinition>)sourceItems, (IEnumerable<CompleteApplicationMetadataDefinition>)enumerable);
            }
            return (IEnumerable<TR>) MergeEntities((IEnumerable<EntityMetadata>)sourceItems, (IEnumerable<EntityMetadata>)enumerable);
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
            foreach (var schema in souceAplication.Schemas()) {
                ApplicationSchemaDefinition overridenSchema;
                overridenApplication.Schemas().TryGetValue(schema.Key, out overridenSchema);
                if (overridenSchema != null) {
                    resultSchemas.Add(schema.Key, overridenSchema);
                } else {
                    resultSchemas.Add(schema.Key, schema.Value);
                }
            }

            foreach (var overridenSchema in overridenApplication.Schemas()) {
                if (souceAplication.Schemas().All(f => !f.Key.Equals(overridenSchema.Key))) {
                    resultSchemas.Add(overridenSchema.Key, overridenSchema.Value);
                }
            }

            IDictionary<string, string> overridenParameters = new Dictionary<string, string>();

            foreach (var parameter in souceAplication.Parameters) {
                string value = parameter.Value;
                if (overridenApplication.Parameters.ContainsKey(parameter.Key)) {
                    value = overridenApplication.Parameters[parameter.Key];
                }
                overridenParameters[parameter.Key] = value;
            }
            IList<DisplayableComponent> resultComponents = new List<DisplayableComponent>();


            var title = overridenApplication.Title ?? souceAplication.Title;
            var entity = overridenApplication.Entity ?? souceAplication.Entity;
            var idFieldName = overridenApplication.IdFieldName ?? souceAplication.IdFieldName;
            var userIdFieldName = overridenApplication.UserIdFieldName ?? souceAplication.UserIdFieldName;
            var service = overridenApplication.Service ?? souceAplication.Service;
            var notifications = overridenApplication.Notifications ?? souceAplication.Notifications;

            return new CompleteApplicationMetadataDefinition(souceAplication.Id, souceAplication.ApplicationName,
                title, entity, idFieldName,userIdFieldName,
                overridenParameters, resultSchemas, souceAplication.DisplayableComponents.Union(overridenApplication.DisplayableComponents), service, notifications);

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
