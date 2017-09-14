using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Metadata.Stereotypes;

namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     stereotype metadata stored in the stereotype.xml file
    /// </summary>
    public sealed class XmlSecurityMetadataParser {
        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all entity metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public Tuple<List<SecurityApplicationEntry>, List<RemoveApplicationEntry>> Parse([NotNull] TextReader stream) {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var document = XDocument.Load(stream);
            if (document.Root == null) {
                throw new InvalidDataException();
            }
            var elementsToIterate = LocateSecurityElement(document);
            var entries = new List<SecurityApplicationEntry>();
            var removeEntries = new List<RemoveApplicationEntry>();
            if (elementsToIterate == null) {
                return new Tuple<List<SecurityApplicationEntry>, List<RemoveApplicationEntry>>(entries, removeEntries);

            }

            foreach (var xElement in elementsToIterate) {
                if (xElement.IsNamed(XmlMetadataSchema.ApplicationElement)) {
                    var entry = ParseSecurityApplication(xElement);
                    entries.Add(entry);
                } else if (xElement.IsNamed(XmlMetadataSchema.RemoveApplicationElement)) {
                    var entry = ParseRemoveApplicationEntry(xElement);
                }
            }



            return new Tuple<List<SecurityApplicationEntry>, List<RemoveApplicationEntry>>(entries, removeEntries);
        }

        private RemoveApplicationEntry ParseRemoveApplicationEntry(XElement xElement) {
            return null;
        }

        [CanBeNull]
        private static IEnumerable<XElement> LocateSecurityElement(XDocument document) {
            if (document.Root == null) {
                return null;
            }


            var firstOrDefault = document.Root.Elements().FirstOrDefault(e => e.IsNamed(XmlMetadataSchema.StereotypesAttribute));
            return firstOrDefault?.Elements();
        }

        private SecurityApplicationEntry ParseSecurityApplication(XElement stereotypeElement) {

            return null;
        }


    }
}