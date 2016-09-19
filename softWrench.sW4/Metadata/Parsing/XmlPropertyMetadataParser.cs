using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata.Properties;

namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     entity metadata stored in a XML file.
    /// </summary>
    public sealed class XmlPropertyMetadataParser {

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

            var dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
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
            if (dictionary.ContainsKey(key)) {
                throw new MetadataException("property {0} already registered review properties.xml".Fmt(key));
            }
            dictionary.Add(key, value);
        }

        private EnvironmentProperties ParseEnvironment(XElement envElement) {
            var envKey = envElement.Attribute(XmlMetadataSchema.ApplicationPropertyKeyAttribute).Value;
            var dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var xElement in envElement.Elements()) {
                AddProperty(xElement, dictionary);
            }
            return new EnvironmentProperties(envKey, dictionary);
        }



    }
}