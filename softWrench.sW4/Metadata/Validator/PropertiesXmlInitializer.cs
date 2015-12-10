using System;
using System.IO;
using cts.commons.portable.Util;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {


    class PropertiesXmlInitializer {


        private const string Properties = "properties.xml";

        internal MetadataProperties Initialize(Stream streamValidator = null) {
            using (var stream = MetadataParsingUtils.GetStream(streamValidator, Properties)) {
                var metadataProperties = new XmlPropertyMetadataParser().Parse(stream);
                metadataProperties.ValidateRequiredProperties();

                var localFilePath = EnvironmentUtil.GetLocalSWFolder() + "properties.xml";
                var fileInfo = new FileInfo(localFilePath);
                if (fileInfo.Exists && fileInfo.Length != 0) {
                    var localStream = new StreamReader(localFilePath);
                    var localProperties = new XmlPropertyMetadataParser().Parse(localStream);
                  

                    metadataProperties.MergeWithLocal(localProperties);

                }

                return metadataProperties;
            }
        }

       
    }
}
