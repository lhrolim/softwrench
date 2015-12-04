using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using cts.commons.Util;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Metadata.Validator;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     entity metadata stored in a XML file.
    /// </summary>
    internal sealed class XmlEntitySourceMetadataParser : IXmlMetadataParser<Tuple<IEnumerable<EntityMetadata>, EntityQueries>> {

        private readonly Boolean _isSWDDB = false;

        public XmlEntitySourceMetadataParser(bool isSWDB) {
            _isSWDDB = isSWDB;
        }

        /// <summary>
        ///     Deseriliazes the specified XML element to its corresponding
        ///     <seealso cref="EntityMetadata"/> representation.
        /// </summary>
        /// <param name="entity">The `entity` element to be deserialized.</param>
        private EntityMetadata ParseEntity(XElement entity) {
            var name = entity.Attribute(XmlMetadataSchema.EntityAttributeName).Value;
            if (_isSWDDB) {
                name = "_" + name;
            }
            var idAttributeName = entity.Attribute(XmlMetadataSchema.EntityAttributeIdAttribute).Value;
            var useridAttributeName = entity.Attribute(XmlMetadataSchema.EntityAttributeUserIdAttribute).ValueOrDefault(idAttributeName);
            var whereClause = entity.Attribute(XmlMetadataSchema.EntityAttributeWhereClause).ValueOrDefault((string)null);
            var parentEntity = entity.Attribute(XmlMetadataSchema.EntityAttributeParentEntity).ValueOrDefault((string)null);
            if (_isSWDDB && parentEntity != null) {
                parentEntity = "_" + parentEntity;
            }
            var associations = XmlAssociationsParser.Parse(name, entity);
            return new EntityMetadata(name,
                XmlSchemaParser.Parse(name, entity, idAttributeName,useridAttributeName, associations.Item2, whereClause, parentEntity),
                associations.Item1,
                XmlConnectorParametersParser.Parse(entity)
                );
        }


        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all entity metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        /// <param name="alreadyParsedTemplates"></param>
        [NotNull]
        public Tuple<IEnumerable<EntityMetadata>, EntityQueries> Parse([NotNull] TextReader stream, ISet<string> alreadyParsedTemplates = null) {
            Validate.NotNull(stream, "stream");
            var document = XDocument.Load(stream);
            if (null == document.Root) throw new InvalidDataException();

            var xElements = document.Root.Elements();

            var enumerable = xElements as XElement[] ?? xElements.ToArray();

            var templates = enumerable.FirstOrDefault(e => e.IsNamed(XmlMetadataSchema.TemplatesElement));


            var entityMetadatas = new List<EntityMetadata>();
            var result = new Tuple<IEnumerable<EntityMetadata>, EntityQueries>(entityMetadatas, new EntityQueries(new Dictionary<string, string>()));

            entityMetadatas.AddRange(XmlTemplateHandler.HandleTemplatesForEntities(templates, _isSWDDB, alreadyParsedTemplates));


            var entities = enumerable.FirstOrDefault(e => e.IsNamed(XmlMetadataSchema.EntitiesElement));

            if (null == entities) {
                return result;
            }
            var customerEntities = (from entitiesEl in entities.Elements().Where(e => e.IsNamed(XmlMetadataSchema.EntityElement))
                                    select ParseEntity(entitiesEl)).ToList();

            var resultEntities =MetadataMerger.MergeEntities(entityMetadatas, customerEntities);
            
            var queries = entities
                .Elements().FirstOrDefault(e => e.IsNamed(XmlMetadataSchema.QueriesElement));
            var resultQueries = new EntityQueries(ParseQueries(queries));
            return new Tuple<IEnumerable<EntityMetadata>, EntityQueries>(resultEntities, resultQueries);
        }



        private Dictionary<string, string> ParseQueries(XElement queries) {
            var result = new Dictionary<string, string>();
            if (queries == null) {
                return result;
            }
            var queriesElements = queries.Elements().Where(e => e.IsNamed(XmlMetadataSchema.QueryElement));
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
                var requiredExpression = attribute.Attribute(XmlBaseSchemaConstants.BaseDisplayableRequiredExpressionAttribute).ValueOrDefault(false);
                var query = attribute.Attribute(XmlMetadataSchema.AttributeQuery).ValueOrDefault((string)null);
                var isAutoGenerated = attribute.Attribute(XmlMetadataSchema.AttributeAttributeAutoGenerated).ValueOrDefault(false);
                var connectorParameters = XmlConnectorParametersParser.Parse(attribute);

                return new EntityAttribute(name, type, requiredExpression, isAutoGenerated, connectorParameters, query);
            }

            /// <summary>
            ///     Deseriliazes the specified XML element to its corresponding
            ///     <seealso cref="EntitySchema"/> representation.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="entity">The `entity` element containing the schema to be deserialized.</param>
            /// <param name="idAttributeName">The name of the attribute that contains the unique identifier of the entity.</param>
            /// <param name="userIdAttributeName">the id that´s going to be seen by the user</param>
            /// <param name="excludeUndeclaredAssociations"></param>
            /// <param name="whereclause"></param>
            /// <param name="parentEntity"></param>
            public static EntitySchema Parse(string name, XElement entity, string idAttributeName,string userIdAttributeName ,bool excludeUndeclaredAssociations, string whereclause, string parentEntity) {
                var attributes = entity.Elements().FirstOrDefault(e => e.IsNamed(XmlMetadataSchema.AttributesElement));
                if (attributes == null) {
                    return new EntitySchema(name, null, idAttributeName,userIdAttributeName, false, excludeUndeclaredAssociations, whereclause, parentEntity, null);
                }
                var entityAttributes = attributes.Elements().Where(e => e.IsNamed(XmlMetadataSchema.AttributeElement)).Select(ParseAttribute).ToList();
                var excludeUndeclared = attributes.Attribute(XmlMetadataSchema.ExcludeUndeclared).ValueOrDefault(false);
                var tuple = new Tuple<Boolean, ICollection<EntityAttribute>>(excludeUndeclared, entityAttributes);
                return new EntitySchema(name, tuple.Item2, idAttributeName,userIdAttributeName, tuple.Item1, excludeUndeclaredAssociations, whereclause, parentEntity, null);
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
                var allowsNull = attribute.Attribute(XmlMetadataSchema.RelationshipAttributeAttributeAllowsNull).ValueOrDefault(false);
                var quoteLiteral = attribute.Attribute(XmlMetadataSchema.RelationshipAttributeAttributeQuoteLiteral).ValueOrDefault(true);
                var primary = attribute.Attribute(XmlMetadataSchema.RelationshipAttributePrimary).ValueOrDefault(false);

                return string.IsNullOrWhiteSpace(literal)
                    ? new EntityAssociationAttribute(to, from, query, primary, allowsNull)
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
                    .Elements().Where(e => e.IsNamed(XmlMetadataSchema.RelationshipAttributeElement))
                    .Select(ParseAssociationAttribute)
                    .ToList();
            }

            /// <summary>
            ///     Deseriliazes the specified XML element to its corresponding
            ///     <seealso cref="EntityAssociation"/> representation.
            /// </summary>
            /// <param name="association">The XML association to parse.</param>
            private static EntityAssociation ParseAssociation(string entityName, XElement association) {
                var qualifier = association.Attribute(XmlMetadataSchema.RelationshipAttributeQualifier).ValueOrDefault((string)null);
                var to = association.Attribute(XmlMetadataSchema.RelationshipAttributeTo).Value;
                var collection = association.Attribute(XmlMetadataSchema.RelationshipAttributeCollection).ValueOrDefault(false);
                var cacheable = association.Attribute(XmlMetadataSchema.RelationshipCacheableAttribute).ValueOrDefault(false);
                var lazy = association.Attribute(XmlMetadataSchema.RelationshipCacheableAttribute).ValueOrDefault(false);
                var reverseLookupAttribute = association.Attribute(XmlMetadataSchema.RelationshipAttributeReverse).ValueOrDefault((string)null);
                var ignorePrimaryAttribute = association.Attribute(XmlMetadataSchema.IgnorePrimaryAttribute).ValueOrDefault(false);
                return new EntityAssociation(qualifier, to, ParseAssociationAttributes(association), collection, cacheable, lazy,reverseLookupAttribute, ignorePrimaryAttribute);
            }

            /// <summary>
            ///     Iterates through the element deserializing all children `association`
            ///     elements to its corresponding <seealso cref="EntityAssociation"/>
            ///     representation.
            /// </summary>
            /// <param name="entity">The `entity` element to parse.</param>
            /// <param name="name"></param>
            public static Tuple<IEnumerable<EntityAssociation>, bool> Parse(string entityName, XContainer entity) {
                var associations =
                    entity.Elements().FirstOrDefault(e => e.IsNamed(XmlMetadataSchema.RelationshipsElement));

                if (null == associations) {
                    return new Tuple<IEnumerable<EntityAssociation>, Boolean>(Enumerable.Empty<EntityAssociation>(), false);
                }

                var entityAssociations = associations
                    .Elements().Where(e => e.IsNamed(XmlMetadataSchema.RelationshipElement))
                    .Select(a=>ParseAssociation(entityName, a))
                    .ToList();
                var excludeUndeclared = associations.Attribute(XmlMetadataSchema.ExcludeUndeclared).ValueOrDefault(false);
                return new Tuple<IEnumerable<EntityAssociation>, Boolean>(entityAssociations, false);
            }
        }

    }
}