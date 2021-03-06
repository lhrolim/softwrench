using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     entity metadata stored in a XML file.
    /// </summary>
    internal sealed class XmlEntitySourceMetadataParser {
        /// <summary>
        ///     Deseriliazes the specified XML element to its corresponding
        ///     <seealso cref="EntityMetadata"/> representation.
        /// </summary>
        /// <param name="entity">The `entity` element to be deserialized.</param>
        private static EntityMetadata ParseEntity(XElement entity) {
            var name = entity.Attribute(XmlMetadataSchema.EntityAttributeName).Value;
            var idAttributeName = entity.Attribute(XmlMetadataSchema.EntityAttributeIdAttribute).Value;
            var whereClause = entity.Attribute(XmlMetadataSchema.EntityAttributeWhereClause).ValueOrDefault((string)null);
            var parentEntity = entity.Attribute(XmlMetadataSchema.EntityAttributeParentEntity).ValueOrDefault((string)null);
            var associations = XmlAssociationsParser.Parse(entity);
            return new EntityMetadata(name,
                XmlSchemaParser.Parse(name,entity, idAttributeName, associations.Item2, whereClause, parentEntity),
                associations.Item1,
                XmlConnectorParametersParser.Parse(entity)
                );
        }

        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all entity metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public Tuple<IReadOnlyCollection<EntityMetadata>, EntityQueries> Parse([NotNull] TextReader stream) {
            if (stream == null) throw new ArgumentNullException("stream");

            var document = XDocument.Load(stream);
            if (null == document.Root) throw new InvalidDataException();

            var entities = document
                .Root
                .Elements().FirstOrDefault(e => e.Name.LocalName == XmlMetadataSchema.EntitiesElement);



            IReadOnlyCollection<EntityMetadata> entityMetadatas;
            if (null == entities) {
                entityMetadatas = new ReadOnlyCollection<EntityMetadata>(Enumerable
                    .Empty<EntityMetadata>()
                    .ToList());
                return new Tuple<IReadOnlyCollection<EntityMetadata>, EntityQueries>(entityMetadatas, new EntityQueries(new Dictionary<string, string>()));
            }
            entityMetadatas = (from entitiesEl in entities.Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.EntityElement)
                               select ParseEntity(entitiesEl)).ToList();
            var queries = entities
                .Elements().FirstOrDefault(e => e.Name.LocalName == XmlMetadataSchema.QueriesElement);
            var resultQueries = new EntityQueries(ParseQueries(queries));
            return new Tuple<IReadOnlyCollection<EntityMetadata>, EntityQueries>(entityMetadatas, resultQueries);
        }

        private Dictionary<string, string> ParseQueries(XElement queries) {
            var result = new Dictionary<string, string>();
            if (queries == null) {
                return result;
            }
            var queriesElements = queries.Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.QueryElement);
            foreach (var queryElement in queriesElements) {
                var key = queryElement.Attribute(XmlMetadataSchema.ApplicationPropertyKeyAttribute).Value;
                var value = queryElement.Attribute(XmlMetadataSchema.ApplicationPropertyValueAttribute).Value;
                result.Add(key, value);
            }
            return result;
        }

        private static class XmlSchemaParser {
            /// <summary>
            ///     Deseriliazes the specified XML element to its corresponding
            ///     <seealso cref="EntityAttribute"/> representation.
            /// </summary>
            /// <param name="attribute">The XML attribute to parse.</param>
            private static EntityAttribute ParseAttribute(XElement attribute) {
                var name = attribute.Attribute(XmlMetadataSchema.AttributeAttributeName).Value;
                var type = attribute.Attribute(XmlMetadataSchema.AttributeAttributeType).Value;
                var isRequired = attribute.Attribute(XmlMetadataSchema.BaseDisplayableRequiredAttribute).ValueOrDefault(false);
                var query = attribute.Attribute(XmlMetadataSchema.AttributeQuery).ValueOrDefault((string)null);
                var isAutoGenerated = attribute.Attribute(XmlMetadataSchema.AttributeAttributeAutoGenerated).ValueOrDefault(false);
                var connectorParameters = XmlConnectorParametersParser.Parse(attribute);

                return new EntityAttribute(name, type, isRequired, isAutoGenerated, connectorParameters, query);
            }

            /// <summary>
            ///     Deseriliazes the specified XML element to its corresponding
            ///     <seealso cref="EntitySchema"/> representation.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="entity">The `entity` element containing the schema to be deserialized.</param>
            /// <param name="idAttributeName">The name of the attribute that contains the unique identifier of the entity.</param>
            /// <param name="excludeUndeclaredAssociations"></param>
            /// <param name="whereclause"></param>
            /// <param name="parentEntity"></param>
            public static EntitySchema Parse(string name, XElement entity, string idAttributeName, bool excludeUndeclaredAssociations, string whereclause, string parentEntity) {
                var attributes = entity.Elements().FirstOrDefault(e => e.Name.LocalName == XmlMetadataSchema.AttributesElement);
                if (attributes == null) {
                    return new EntitySchema(name,null, idAttributeName, false, excludeUndeclaredAssociations, whereclause, parentEntity);
                }
                var entityAttributes = attributes.Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.AttributeElement).Select(ParseAttribute).ToList();
                var excludeUndeclared = attributes.Attribute(XmlMetadataSchema.ExcludeUndeclared).ValueOrDefault(false);
                var tuple = new Tuple<Boolean, ICollection<EntityAttribute>>(excludeUndeclared, entityAttributes);
                return new EntitySchema(name,tuple.Item2, idAttributeName, tuple.Item1, excludeUndeclaredAssociations, whereclause, parentEntity);
            }
        }

        public static class XmlAssociationsParser {
            /// <summary>
            ///     Deseriliazes the specified XML element to its corresponding
            ///     <seealso cref="EntityAssociationAttribute"/> representation.
            /// </summary>
            /// <param name="attribute">The XML association attribute to parse.</param>
            private static EntityAssociationAttribute ParseAssociationAttribute(XElement attribute) {
                var to = attribute.Attribute(XmlMetadataSchema.RelationshipAttributeAttributeTo).ValueOrDefault((string)null);
                var from = attribute.Attribute(XmlMetadataSchema.RelationshipAttributeAttributeFrom).ValueOrDefault((string)null);
                var query = attribute.Attribute(XmlMetadataSchema.AttributeQuery).ValueOrDefault((string)null);
                var literal = attribute.Attribute(XmlMetadataSchema.RelationshipAttributeAttributeLiteral).ValueOrDefault((string)null);
                var quoteLiteral = attribute.Attribute(XmlMetadataSchema.RelationshipAttributeAttributeQuoteLiteral).ValueOrDefault(true);
                var primary = attribute.Attribute(XmlMetadataSchema.RelationshipAttributePrimary).ValueOrDefault(false);

                return string.IsNullOrWhiteSpace(literal)
                    ? new EntityAssociationAttribute(to, from, query, primary)
                    : new EntityAssociationAttribute(quoteLiteral, to, from, literal);
            }

            /// <summary>
            ///     Iterates through the element deserializing all children `associationAttribute`
            ///     elements to its corresponding <seealso cref="EntityAssociationAttribute"/>
            ///     representation.
            /// </summary>
            /// <param name="association">The `association` element to parse.</param>
            public static IEnumerable<EntityAssociationAttribute> ParseAssociationAttributes(XContainer association) {
                return association
                    .Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.RelationshipAttributeElement)
                    .Select(ParseAssociationAttribute)
                    .ToList();
            }

            /// <summary>
            ///     Deseriliazes the specified XML element to its corresponding
            ///     <seealso cref="EntityAssociation"/> representation.
            /// </summary>
            /// <param name="association">The XML association to parse.</param>
            private static EntityAssociation ParseAssociation(XElement association) {
                var qualifier = association.Attribute(XmlMetadataSchema.RelationshipAttributeQualifier).ValueOrDefault((string)null);
                var to = association.Attribute(XmlMetadataSchema.RelationshipAttributeTo).Value;
                var collection = association.Attribute(XmlMetadataSchema.RelationshipAttributeCollection).ValueOrDefault(false);
                return new EntityAssociation(qualifier, to, ParseAssociationAttributes(association), collection);
            }

            /// <summary>
            ///     Iterates through the element deserializing all children `association`
            ///     elements to its corresponding <seealso cref="EntityAssociation"/>
            ///     representation.
            /// </summary>
            /// <param name="entity">The `entity` element to parse.</param>
            public static Tuple<IEnumerable<EntityAssociation>, Boolean> Parse(XContainer entity) {
                var associations =
                    entity.Elements().FirstOrDefault(e => e.Name.LocalName == XmlMetadataSchema.RelationshipsElement);

                if (null == associations) {
                    return new Tuple<IEnumerable<EntityAssociation>, Boolean>(Enumerable.Empty<EntityAssociation>(), false);
                }

                var entityAssociations = associations
                    .Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.RelationshipElement)
                    .Select(ParseAssociation)
                    .ToList();
                var excludeUndeclared = associations.Attribute(XmlMetadataSchema.ExcludeUndeclared).ValueOrDefault(false);
                return new Tuple<IEnumerable<EntityAssociation>, Boolean>(entityAssociations, false);
            }
        }

    }
}