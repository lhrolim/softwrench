using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using softWrench.sW4.Metadata.Entities.Connectors;

namespace softWrench.sW4.Metadata.Parsing {
    internal class XmlConnectorParametersParser {

        /// <summary>
        ///     Deseriliazes the specified XML element to its corresponding
        ///     <seealso cref="KeyValuePair{TKey,TValue}"/> representation.
        /// </summary>
        /// <param name="parameter">The XML connector parameter to parse.</param>
        private static KeyValuePair<string, string> ParseParameter(XElement parameter) {
            var key = parameter.Attribute(XmlMetadataSchema.ConnectorParameterAttributeKey).Value;
            var value = parameter.Attribute(XmlMetadataSchema.ConnectorParameterAttributeValue).Value;

            return new KeyValuePair<string, string>(key, value);
        }

        /// <summary>
        ///     Iterates through the element deserializing all children `connectorParameter`
        ///     elements to its corresponding <seealso cref="KeyValuePair{TKey,TValue}"/>
        ///     representation.
        /// </summary>
        /// <param name="parameters">The `connectorParameters` element to parse.</param>
        private static IDictionary<string, string> ParseParameters(XContainer parameters) {
            return parameters
                .Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.ConnectorParameterElement)
                .Select(ParseParameter)
                .ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        ///     Deseriliazes the specified XML element to its corresponding
        ///     <seealso cref="ConnectorParameters"/> representation.
        /// </summary>
        /// <param name="element">The element containing the connector parameters to be deserialized.</param>
        public static ConnectorParameters Parse(XElement element) {
            var parameters = element.Elements().FirstOrDefault(e => e.Name.LocalName == XmlMetadataSchema.ConnectorParametersElement);
            var excludeUndeclared = element.Attribute(XmlMetadataSchema.ExcludeUndeclared).ValueOrDefault(false);
            return null == parameters
                ? ConnectorParameters.DefaultInstance()
                : new ConnectorParameters(ParseParameters(parameters), excludeUndeclared);
        }

    }
}
