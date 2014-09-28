using System;
using System.IO;
using log4net;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {
    class MetadataParsingUtils {


        private const string ClientMetadataPattern = "\\App_Data\\Client\\{0}\\";
        private const string InternalMetadataPattern = "\\App_Data\\Client\\@internal\\{0}\\{1}.xml";
        private const string TestInternalMetadataPattern = "\\Client\\@internal\\{0}\\{1}.xml";
        private const string TestMetadataPath = "\\Client\\{0}\\";

        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataParsingUtils));

        public static string GetPath(string resource, bool internalFramework = false) {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var clientName = internalFramework ? "@internal" : ApplicationConfiguration.ClientName;
            var pattern = ApplicationConfiguration.IsUnitTest ? TestMetadataPath : ClientMetadataPattern;
            return (baseDirectory + String.Format(pattern, clientName) + resource);
        }


        public static string GetEntitiesInternalPath(bool source = false) {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var type = source ? "source" : "target";
            var propertyname = source ? MetadataProperties.Source : MetadataProperties.Target;
            var pattern = ApplicationConfiguration.IsUnitTest ? TestInternalMetadataPattern : InternalMetadataPattern;
            var mapping = MetadataProvider.GlobalProperty(propertyname);
            if (type == "target" && mapping == null) {
                //as a fallback, we will look for the same file as the source
                mapping = MetadataProvider.GlobalProperty("sourcemapping");
            }
            return (baseDirectory + String.Format(pattern, type, mapping));
        }


        public static StreamReader GetStream(Stream streamValidator, String path) {
            return streamValidator == null ? GetStreamImpl(path) : new StreamReader(StreamUtils.CopyStream(streamValidator));
        }

        public static StreamReader GetStreamImpl(string resource, bool internalFramework = false, Stream streamValidator = null) {
            if (streamValidator != null) {
                return new StreamReader(StreamUtils.CopyStream(streamValidator));
            }
            try {
                var path = GetPath(resource, internalFramework);
                if (File.Exists(path)) {
                    return new StreamReader(path);
                }
                return null;
            } catch (Exception) {
                //nothing to do here.
                return null;
            }
        }

        internal static StreamReader GetInternalStreamImpl(bool source, Stream streamValidator = null) {
            if (streamValidator != null) {
                return new StreamReader(StreamUtils.CopyStream(streamValidator));
            }
            return new StreamReader(GetEntitiesInternalPath(source));
        }







    }
}
