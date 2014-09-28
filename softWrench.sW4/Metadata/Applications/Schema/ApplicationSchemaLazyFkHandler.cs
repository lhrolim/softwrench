using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Util;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Metadata.Applications.Schema {
    class ApplicationSchemaLazyFkHandler {

        public static ApplicationSchemaDefinition.LazyFkResolverDelegate SyncSchemaLazyFkResolverDelegate =
            delegate(ApplicationSchemaDefinition definition) {
                if (!MetadataProvider.FinishedParsing) {
                    return null;
                }
                var app = MetadataProvider.Application(definition.ApplicationName);
                var schemas = app.Schemas();
                var resultSet =
                    new HashSet<IApplicationAttributeDisplayable>();
                foreach (var schema in schemas.Values) {
                    if (Equals(schema.GetSchemaKey(), definition.GetSchemaKey()) || schema.IsWebPlatform()) {
                        continue;
                    }
                    var fields = schema.Fields;
                    foreach (var applicationFieldDefinition in fields) {
                        resultSet.Add(applicationFieldDefinition);
                    }
                }
                return new List<IApplicationAttributeDisplayable>(resultSet);
            };


        public static ApplicationSchemaDefinition.LazyFkResolverDelegate LazyFkResolverDelegate = delegate(ApplicationSchemaDefinition definition) {
            if (!MetadataProvider.FinishedParsing) {
                return null;
            }
            if (definition.Stereotype != SchemaStereotype.List &&
                definition.Stereotype != SchemaStereotype.CompositionList) {
                //blank list in order to consider it done
                return new List<IApplicationAttributeDisplayable>();
            }
            var resultList = new List<IApplicationAttributeDisplayable>();
            resultList.AddRange(DirectFKsFields(definition));
            resultList.AddRange(InverseFKsFields(definition));
            return resultList;
        };

        private static IEnumerable<IApplicationAttributeDisplayable> InverseFKsFields(ApplicationSchemaDefinition thisSchema) {
            var resultList = new List<IApplicationAttributeDisplayable>();
            var applications = MetadataProvider.Applications();
            foreach (var metadata in applications) {
                foreach (var schema in metadata.Schemas().Values) {
                    if (schema.Stereotype != SchemaStereotype.Detail) {
                        continue;
                    }
                    var reverseComposition = schema.Compositions.FirstOrDefault(f => f.Relationship == EntityUtil.GetRelationshipName(thisSchema.ApplicationName));
                    if (reverseComposition != null) {
                        var association = reverseComposition.EntityAssociation;
                        foreach (var attribute in association.Attributes) {
                            if (attribute.To != null) {
                                resultList.Add(ApplicationFieldDefinition.HiddenInstance(thisSchema.ApplicationName,
                                    attribute.To));
                            }
                        }
                    }
                    var reverseAssociation = schema.Associations.FirstOrDefault(f => f.EntityAssociation.Qualifier == thisSchema.ApplicationName);
                    if (reverseAssociation != null) {
                        var association = reverseAssociation.EntityAssociation;
                        foreach (var attribute in association.Attributes) {
                            if (attribute.To != null) {
                                resultList.Add(ApplicationFieldDefinition.HiddenInstance(thisSchema.ApplicationName,
                                    attribute.To));
                            }
                        }
                    }

                }
            }
            return resultList;
        }

        private static IEnumerable<IApplicationAttributeDisplayable> DirectFKsFields(ApplicationSchemaDefinition schema) {
            return new List<IApplicationAttributeDisplayable>();
        }



    }
}
