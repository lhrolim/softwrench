using System.Diagnostics;
using System.Globalization;
using cts.commons.portable.Util;
using log4net;
using softwrench.sw4.Shared2.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Metadata.Entities.Sliced {
    public class SlicedEntityMetadataBuilder {
        private const string MissingAssociation = "couldn´t find association {0} on entity {1}. Please, review metadata.xml";

        private static readonly ILog Log = LogManager.GetLogger(typeof(SlicedEntityMetadataBuilder));

        public static SlicedEntityMetadata GetInstance(EntityMetadata entityMetadata,
                                                     ApplicationSchemaDefinition appSchema, int? fetchLimit = 300, bool isUnionSchema = false) {
            var entityAttributes = entityMetadata.Schema.Attributes;
            var usedRelationships = new HashSet<EntityAssociation>();
            var watch = Stopwatch.StartNew();
           
            ISet<EntityAttribute> usedAttributes = new HashSet<EntityAttribute>();
            var nonRelationshipFields =appSchema.NonRelationshipFields;
            foreach (var field in nonRelationshipFields) {
                if (field.Attribute.StartsWith("#null")) {
                    usedAttributes.Add(new EntityAttribute(field.Attribute, "varchar", false, true,
                        ConnectorParameters.DefaultInstance(), null));
                } else {
                    var entityAttribute = entityAttributes.FirstOrDefault(r => field.Attribute.EqualsIc(r.Name));
                    if (entityAttribute != null) {
                        usedAttributes.Add(entityAttribute);
                    }
                }
            }


            usedAttributes.Add(entityMetadata.Schema.IdAttribute);
            if (!isUnionSchema && !entityMetadata.SWEntity()) {
                usedAttributes.Add(entityMetadata.Schema.RowstampAttribute);
            }

            usedRelationships.UnionWith(HandleAssociations(appSchema.Associations(), entityMetadata));
            usedRelationships.UnionWith(HandleCompositions(appSchema.Compositions(), entityMetadata, appSchema));


            var result = SlicedRelationshipBuilderHelper.HandleRelationshipFields(appSchema.RelationshipFields.Select(r => r.Attribute), entityMetadata);

            usedRelationships.UnionWith(result.DirectRelationships);
            // When should the rowstamp be excluded
            var schema = new EntitySchema(entityMetadata.Name, usedAttributes, entityMetadata.Schema.IdAttribute.Name, entityMetadata.Schema.UserIdAttribute.Name, false, false, entityMetadata.Schema.WhereClause, entityMetadata.Schema.ParentEntity, entityMetadata.Schema.MappingType, (!isUnionSchema && !entityMetadata.SWEntity()));
            SlicedEntityMetadata unionSchema = null;
            if (appSchema.UnionSchema != null) {
                unionSchema = GetUnionInstance(appSchema.UnionSchema);
            }


            Log.DebugFormat("Finished Building Sliced Metadata for {0} in {1} ms", appSchema, watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
            watch.Stop();
            return new SlicedEntityMetadata(entityMetadata.Name, schema,
                usedRelationships, entityMetadata.ConnectorParameters, appSchema, result.InnerEntityMetadatas, fetchLimit, unionSchema);
        }


        private static SlicedEntityMetadata GetUnionInstance(string unionSchema) {
            var appAndSchema = SchemaUtil.ParseApplicationAndKey(unionSchema);
            var appName = appAndSchema.Item1;
            var app = MetadataProvider.Application(appName);
            var entityName = app.Entity;
            var entity = MetadataProvider.Entity(entityName);
            var schema = app.Schema(appAndSchema.Item2);
            return GetInstance(entity, schema, 300, true);
        }

        public static SlicedEntityMetadata GetInnerInstance(EntityMetadata entityMetadata, IEnumerable<string> attributes) {
            var entityAttributes = entityMetadata.Schema.Attributes;
            var usedRelationships = new HashSet<EntityAssociation>();

            ISet<EntityAttribute> usedAttributes = new HashSet<EntityAttribute>(
                     entityAttributes
                        .Where(attribute => attributes
                        .Any(r => r == attribute.Name))
                     .ToList());

            usedAttributes.Add(entityMetadata.Schema.IdAttribute);
            usedAttributes.Add(entityMetadata.Schema.RowstampAttribute);

            var result = SlicedRelationshipBuilderHelper.HandleRelationshipFields(attributes.Where(r => r.Contains('.')), entityMetadata);
            usedRelationships.UnionWith(result.DirectRelationships);
            var schema = new EntitySchema(entityMetadata.Name, usedAttributes, entityMetadata.Schema.IdAttribute.Name, entityMetadata.Schema.UserIdAttribute.Name, false, false, entityMetadata.Schema.WhereClause, entityMetadata.Schema.ParentEntity, entityMetadata.Schema.MappingType);
            return new SlicedEntityMetadata(entityMetadata.Name, schema,
                usedRelationships, entityMetadata.ConnectorParameters, null, result.InnerEntityMetadatas);
        }





        private static IEnumerable<EntityAssociation> HandleCompositions(IEnumerable<ApplicationCompositionDefinition> compositions, EntityMetadata entityMetadata, ApplicationSchemaDefinition appSchema) {
            return
                compositions.Where(c=> !c.Relationship.StartsWith("#")).Select(
                composition => {
                    var entityAssociation = entityMetadata.Associations.FirstOrDefault(a => a.Qualifier == composition.Relationship);
                    if (entityAssociation == null) {
                        throw new InvalidOperationException(String.Format(MissingAssociation, composition.Relationship, entityMetadata.Name));
                    }
                    CompositionBuilder.InitializeCompositionSchemas(appSchema);
                    if (composition.Schema.Schemas == null) {
                        return entityAssociation;
                    }
                    ApplicationSchemaDefinition schemaToUse = composition.Collection ? composition.Schema.Schemas.List : composition.Schema.Schemas.Detail;
                    if (schemaToUse == null) {
                        return entityAssociation;
                    }
                    var entity = MetadataProvider.Entity(entityAssociation.To);
                    var slicedAttributes = entity.Attributes(EntityMetadata.AttributesMode.NoCollections)
                        .Where(attribute => schemaToUse.Fields
                            .Any(r => r.Attribute.Equals(attribute.Name)));

                    return new SlicedEntityAssociation(entityAssociation, slicedAttributes);
                })
                .ToList();
        }

        private static IEnumerable<SlicedEntityAssociation> HandleAssociations(IEnumerable<ApplicationAssociationDefinition> associations,
            EntityMetadata entityMetadata) {
            var usedRelationships = new List<SlicedEntityAssociation>();
            foreach (var association in associations) {
                var entityAssociation = association.EntityAssociation;
                if (entityAssociation == null) {
                    throw ExceptionUtil.InvalidOperation(MissingAssociation, association.Attribute, entityMetadata.Name);
                    //                    throw new InvalidOperationException(String.Format(MissingAssociation, entityAssociation.Qualifier, entityMetadata.Name));
                }
                if (entityAssociation.Reverse) {
                    Log.DebugFormat("ignoring reverse mapping {0}", association);
                    continue;
                }
                var usedAttributes = new HashSet<string>();
                var labelFields = association.LabelFields;
                usedAttributes.AddAll(labelFields);
                // usedAttributes.AddAll(entityAssociation.Attributes.Select(a => a.To));

                var entity = MetadataProvider.Entity(entityAssociation.To);
                var slicedAttributes = entity.Attributes(EntityMetadata.AttributesMode.NoCollections)
                      .Where(attribute => labelFields
                          .Any(r => r.Equals(attribute.Name)));

                usedRelationships.Add(new SlicedEntityAssociation(entityAssociation, slicedAttributes, entityAssociation.Qualifier));
            }
            return usedRelationships;
        }
    }
}
