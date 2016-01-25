using System;
using System.Collections.Generic;
using System.IO;
using cts.commons.portable.Util;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Metadata.Stereotypes;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {


    class StereotypesXmlInitializer {


        private const string Stereotypes = "stereotypes.xml";

        internal IDictionary<string,MetadataStereotype> Initialize(Stream streamValidator = null) {

            using (var stream = MetadataParsingUtils.GetStream(streamValidator, Stereotypes)) {
                var stereotypes = new XmlStereotypeMetadataParser().Parse(stream,false);
                return stereotypes;
            }
        }

       
    }
}
