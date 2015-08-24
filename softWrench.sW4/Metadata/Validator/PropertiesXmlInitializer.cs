﻿using System;
using System.IO;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Properties;

namespace softWrench.sW4.Metadata.Validator {


    class PropertiesXmlInitializer {


        private const string Properties = "properties.xml";

        internal MetadataProperties Initialize(Stream streamValidator = null) {
            using (var stream = MetadataParsingUtils.GetStream(streamValidator, Properties)) {
                var metadata = new XmlPropertyMetadataParser().Parse(stream);
                metadata.ValidateRequiredProperties();

                var localFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\properties.xml";
                var fileInfo = new FileInfo(localFilePath);
                if (fileInfo.Exists && fileInfo.Length != 0) {
                    var localStream = new StreamReader(localFilePath);
                    var localProperties = new XmlPropertyMetadataParser().Parse(localStream);
                    foreach (var property in localProperties.Properties) {
                        metadata.Properties[property.Key] = property.Value;
                    }
                }

                return metadata;
            }
        }


    }
}
