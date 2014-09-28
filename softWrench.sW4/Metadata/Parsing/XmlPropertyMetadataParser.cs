using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Metadata.Properties;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     entity metadata stored in a XML file.
    /// </summary>
    internal sealed class XmlPropertyMetadataParser {

        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all entity metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public MetadataProperties Parse([NotNull] TextReader stream) {
            if (stream == null) throw new ArgumentNullException("stream");

            var document = XDocument.Load(stream);
            if (null == document.Root) {
                throw new InvalidDataException();
            }

            var dictionary = new Dictionary<string, string>();
            IList<EnvironmentProperties> environments = new List<EnvironmentProperties>();
            foreach (var xElement in document.Root.Elements()) {
                if (xElement.Name.LocalName == XmlMetadataSchema.EnvironmentElement) {
                    environments.Add(ParseEnvironment(xElement));
                } else {
                    AddProperty(xElement, dictionary);
                }
            }

            return new MetadataProperties(dictionary, environments);
        }

        private static void AddProperty(XElement xElement, Dictionary<string, string> dictionary) {
            var key = xElement.Attribute(XmlMetadataSchema.PropertyKeyAttribute).Value;
            var value = xElement.Attribute(XmlMetadataSchema.PropertyValueAttribute).Value;
            dictionary.Add(key, value);
        }

        private EnvironmentProperties ParseEnvironment(XElement envElement) {
            var envKey = envElement.Attribute(XmlMetadataSchema.ApplicationPropertyKeyAttribute).Value;
            var dictionary = new Dictionary<string, string>();
            foreach (var xElement in envElement.Elements()) {
                AddProperty(xElement, dictionary);
            }
            return new EnvironmentProperties(envKey, dictionary);
        }



    }
}