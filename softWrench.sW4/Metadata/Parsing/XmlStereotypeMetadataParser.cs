using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Metadata.Stereotypes;

namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     stereotype metadata stored in the stereotype.xml file
    /// </summary>
    public sealed class XmlStereotypeMetadataParser {

        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all entity metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public IDictionary<string, MetadataStereotype> Parse([NotNull] TextReader stream) {
            if (stream == null) throw new ArgumentNullException("stream");

            var document = XDocument.Load(stream);
            if (null == document.Root) {
                throw new InvalidDataException();
            }
            var stereotypes = new Dictionary<string, MetadataStereotype>();
            foreach (var xElement in document.Root.Elements()) {
                if (xElement.Name.LocalName == XmlMetadataSchema.SchemaStereotypeAttribute) {
                    var stereotype = ParseStereotype(xElement);
                    stereotypes.Add(stereotype.Id, stereotype);
                }
            }
            return stereotypes;
        }

        private MetadataStereotype ParseStereotype(XElement stereotypeElement) {
            var id = stereotypeElement.Attribute(XmlBaseSchemaConstants.IdAttribute).Value;
            var properties = new Dictionary<string, string>();
            foreach (var propertyEl in stereotypeElement.Elements()) {
                if (propertyEl.IsNamed(XmlMetadataSchema.PropertyElement)) {
                    var key = propertyEl.Attribute(XmlMetadataSchema.PropertyKeyAttribute).Value;
                    var value = propertyEl.Attribute(XmlMetadataSchema.PropertyValueAttribute).ValueOrDefault((string)null);
                    properties.Add(key, value);
                }
            }
            return new MetadataStereotype(id, properties);
        }


    }
}