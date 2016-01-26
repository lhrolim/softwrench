using System.Collections.Generic;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Parsing;

namespace softWrench.sW4.Metadata.Validator {
    internal class MetadataXmlSourceInitializer : BaseMetadataXmlSourceInitializer {



        protected override string MetadataPath() {
            return "metadata.xml";
        }

        protected override bool IsSWDB() {
            return false;
        }

        protected override IEnumerable<EntityMetadata> InitializeEntityInternalMetadata() {
            using (var stream = MetadataParsingUtils.GetInternalStreamImpl(true)) {
                return new XmlEntitySourceMetadataParser(false,false).Parse(stream).Item1;
            }
        }

    }
}
