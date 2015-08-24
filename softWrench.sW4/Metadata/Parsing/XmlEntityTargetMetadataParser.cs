using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;

namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     entity metadata stored in a XML file.
    /// </summary>
    internal sealed class XmlEntityTargetMetadataParser {

        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all entity metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public IDictionary<string, TargetParsingResult> Parse([NotNull] TextReader stream) {
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }

            var document = XDocument.Load(stream);
            if (null == document.Root) {
                throw new InvalidDataException();
            }
            var resultDict = new Dictionary<string, TargetParsingResult>();
            foreach (var element in document.Root.Elements()) {
                var result = Parse(element);
                resultDict.Add(result.Key, result.Value);
            }
            return resultDict;
        }

        internal class TargetParsingResult {
            internal EntityTargetSchema TargetSchema;
            internal ConnectorParameters Parameters;

            public TargetParsingResult(EntityTargetSchema targetSchema, ConnectorParameters parameters) {
                this.TargetSchema = targetSchema;
                this.Parameters = parameters;
            }
        }

        private KeyValuePair<string, TargetParsingResult> Parse(XElement entityElement) {
            var name = entityElement.Attribute(XmlMetadataSchema.AttributeAttributeName).Value;
            var attributes = entityElement.Elements().First(e => e.Name.LocalName == XmlMetadataSchema.AttributesElement);
            var value = DoParseSchema(entityElement, attributes);
            var connectorParameters = ParseConnectorParameters(entityElement);
            return new KeyValuePair<string, TargetParsingResult>(name, new TargetParsingResult(value, connectorParameters));
        }

        private ConnectorParameters ParseConnectorParameters(XElement entityElement) {
            return XmlConnectorParametersParser.Parse(entityElement);
        }

        private EntityTargetSchema DoParseSchema(XElement entityElement, XElement attributes) {
            var constValues = new HashSet<EntityTargetConstant>();
            var targetAttributes = new HashSet<EntityTargetAttribute>();
            foreach (var element in attributes.Elements()) {
                if (element.Name.LocalName == XmlMetadataSchema.ConstElement) {
                    var keyValue = ParseConstValue(element);
                    constValues.Add(keyValue);
                } else if (element.Name.LocalName == XmlMetadataSchema.AttributeElement) {
                    targetAttributes.Add(ParseTargetAttributes(element));
                }
            }
            var value = new EntityTargetSchema(constValues, targetAttributes, ParseRelationships(entityElement));
            return value;
        }

        private IEnumerable<EntityTargetRelationship> ParseRelationships(XElement entityElement) {
            var result = new List<EntityTargetRelationship>();

            var relationships = entityElement.Elements().FirstOrDefault(e => e.Name.LocalName == XmlMetadataSchema.RelationshipsElement);
            if (relationships == null) {
                return result;
            }
            foreach (var relationship in relationships.Elements()) {
                var targetPath = relationship.Attribute(XmlMetadataSchema.TargetPathAttribute).Value;
                var attribute = relationship.Attribute(XmlMetadataSchema.AttributeElement).Value;
                result.Add(new EntityTargetRelationship(DoParseSchema(relationship, relationship), targetPath, attribute));
            }
            return result;
        }

        private EntityTargetConstant ParseConstValue(XElement element) {
            var targetPath = element.Attribute(XmlMetadataSchema.TargetPathAttribute).Value;
            var value = element.Attribute(XmlMetadataSchema.PropertyValueAttribute).Value;
            return new EntityTargetConstant
            {
                Key = element.Attribute(XmlMetadataSchema.TargetPathAttribute).Value,
                Value = element.Attribute(XmlMetadataSchema.PropertyValueAttribute).Value,
                Type = element.Attribute(XmlMetadataSchema.AttributeAttributeType).ValueOrDefault("varchar")

            };

        }

        private EntityTargetAttribute ParseTargetAttributes(XElement element) {
            var name = element.Attribute(XmlMetadataSchema.AttributeAttributeName).Value;
            var type = element.Attribute(XmlMetadataSchema.AttributeAttributeType).Value;
            var requiredExpression = element.Attribute(XmlBaseSchemaConstants.BaseDisplayableRequiredExpressionAttribute).ValueOrDefault(false);
            var connectorParameters = XmlConnectorParametersParser.Parse(element);
            var targetPath = element.Attribute(XmlMetadataSchema.TargetPathAttribute).ValueOrDefault("");
            return new EntityTargetAttribute(name, type, requiredExpression, connectorParameters, targetPath);
        }


    }
}