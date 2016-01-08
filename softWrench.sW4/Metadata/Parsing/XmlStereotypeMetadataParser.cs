using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// <param name="metadataParser">whether we are running the parser on stereotypes.xml or metadata.xml</param>
        [NotNull]
        public IDictionary<string, MetadataStereotype> Parse([NotNull] TextReader stream, bool metadataParser) {
            if (stream == null) throw new ArgumentNullException("stream");

            var document = XDocument.Load(stream);
            if (document.Root == null) {
                throw new InvalidDataException();
            }
            var stereotypes = new SortedDictionary<string, MetadataStereotype>();
            var elementsToIterate = LocateStereotypesElement(document, metadataParser);
            if (elementsToIterate == null) {
                return stereotypes;

            }
            foreach (var xElement in elementsToIterate) {
                if (xElement.IsNamed(XmlMetadataSchema.SchemaStereotypeAttribute)) {
                    var stereotype = ParseStereotype(xElement);
                    stereotypes.Add(stereotype.Id, stereotype);
                }
            }

            var composedStereotypes = new List<MetadataStereotype>(stereotypes.Values.Where(r => r.Id.Contains(".")));

            foreach (var composedStereotype in composedStereotypes) {
                var originalStereotype =
                    stereotypes.Values.LastOrDefault(f => composedStereotype.Id.StartsWith(f.Id) && !composedStereotype.Id.Equals(f.Id));
                if (originalStereotype != null) {
                    stereotypes[composedStereotype.Id] = (MetadataStereotype)originalStereotype.Merge(composedStereotype);
                }
            }



            return stereotypes;
        }

        [CanBeNull]
        private static IEnumerable<XElement> LocateStereotypesElement(XDocument document, bool metadataParser) {
            if (document.Root == null) {
                return null;
            }

            if (metadataParser) {
                var firstOrDefault = document.Root.Elements().FirstOrDefault(e => e.IsNamed(XmlMetadataSchema.StereotypesAttribute));
                if (firstOrDefault != null) {
                    return firstOrDefault.Elements();
                }
                return null;
            }

            return document.Root.Elements();
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