using System;
using System.IO;
using cts.commons.Util;
using log4net;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {
    class MetadataParsingUtils {


        private const string ClientMetadataPattern = "\\App_Data\\Client\\{0}\\";
        internal const string TemplatesInternalPath = "\\App_Data\\Client\\@internal\\templates\\{0}";
        internal const string TestTemplatesInternalPath = "\\Client\\@internal\\templates\\{0}";
        private const string InternalMetadataPattern = "\\App_Data\\Client\\@internal\\{0}\\{1}.xml";
        private const string TestInternalMetadataPattern = "\\Client\\@internal\\{0}\\{1}.xml";
        private const string TestMetadataPath = "\\Client\\{0}\\";
        private const string TestMetadataModulePath = "\\metadata\\{0}\\";
        private const string OtbPath = "\\App_Data\\Client\\otb\\";

        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataParsingUtils));

        public static string GetPath(string resource, bool internalFramework = false, bool otbpath = false) {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var clientName = otbpath ? "otb" : ApplicationConfiguration.ClientName;
            if (internalFramework) {
                clientName = "@internal";
            }
            var pattern = ApplicationConfiguration.IsUnitTest ? TestMetadataPath : ClientMetadataPattern;
            return @"" + (baseDirectory + String.Format(pattern, clientName) + resource);
        }

        public static string GetPathForUnitTestModule(string resource, bool internalFramework = false, bool otbpath = false) {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var clientName = otbpath ? "otb" : ApplicationConfiguration.ClientName;
            if (internalFramework) {
                clientName = "@internal";
            }
            return @"" + (baseDirectory + String.Format(TestMetadataModulePath, clientName) + resource);
        }

        public static Stream GetStreamFromCustomerDll(string resource) {
            var assembly = AssemblyLocator.GetCustomerAssembly();
            var resourceName = String.Format("softwrench.sw4.{0}.metadata.{0}.{1}", ApplicationConfiguration.ClientName, resource);
            return assembly.GetManifestResourceStream(resourceName);
        }

      

        public static string GetTemplateInternalPath(string resource) {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (resource.StartsWith("@")) {
                resource = resource.Substring(1);
                if (ApplicationConfiguration.IsUnitTest) {
                    return @"" + baseDirectory + TestTemplatesInternalPath.Fmt(resource);
                }
                return @"" + baseDirectory + TemplatesInternalPath.Fmt(resource);
            }
            if (ApplicationConfiguration.IsUnitTest) {
                return @"" + baseDirectory + TestMetadataPath.Fmt(ApplicationConfiguration.ClientName) + resource;
            }

            return @"" + baseDirectory + ClientMetadataPattern.Fmt(ApplicationConfiguration.ClientName) + resource;
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

        public static StreamReader DoGetStream(string path) {
            try {
                if (File.Exists(path)) {
                    return new StreamReader(path);
                }
                return null;
            } catch (Exception) {
                //nothing to do here.
                return null;
            }
        }

        public static StreamReader GetStreamImpl(string resource, Stream streamValidator = null) {
            if (streamValidator != null) {
                return new StreamReader(StreamUtils.CopyStream(streamValidator));
            }
            try {
                var path = GetPath(resource);
                if (File.Exists(path)) {
                    return new StreamReader(path);
                }
                if (ApplicationConfiguration.IsUnitTest)
                {
                    path = GetPathForUnitTestModule(resource);
                    if (File.Exists(path)) {
                        return new StreamReader(path);
                    }
                }

                if (!ApplicationConfiguration.IsUnitTest && AssemblyLocator.CustomerAssemblyExists()) {
                    //we cannot call the assembly locator in unit test context
                    var stream = GetStreamFromCustomerDll(resource);
                    if (stream != null) {
                        return new StreamReader(stream);
                    }
                }
                Log.InfoFormat("getting file {0} from otb default implementation",path);
                path = GetPath(resource, false, true);
                if (!File.Exists(path)) {
                    return null;
                }
                return new StreamReader(path);
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
