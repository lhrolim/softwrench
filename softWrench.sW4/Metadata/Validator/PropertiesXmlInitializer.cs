using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {


    class PropertiesXmlInitializer {


        private const string Properties = "properties.xml";

        internal MetadataProperties Initialize(Stream streamValidator = null) {
            using (var stream = MetadataParsingUtils.GetStream(streamValidator, Properties)) {
                var metadata = new XmlPropertyMetadataParser().Parse(stream);
                metadata.ValidateRequiredProperties();
                return metadata;
            }
        }


    }
}
