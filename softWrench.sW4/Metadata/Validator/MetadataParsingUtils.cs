using System;
using System.IO;
using cts.commons.portable.Util;
using log4net;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {
    public class MetadataParsingUtils {


        private const string ClientMetadataPattern = "\\App_Data\\Client\\{0}\\";
        internal const string TemplatesInternalPath = "\\App_Data\\Client\\@internal\\templates\\{0}";
        internal const string TemplatesSWDBInternalPath = "\\App_Data\\Client\\@internal\\templates\\swdb\\{0}";
        internal const string MenuTemplatesInternalPath = "\\App_Data\\Client\\@internal\\templates\\menu\\menutemplate.{0}.xml";
        internal const string TestMenuTemplatesInternalPath = "\\Client\\@internal\\templates\\menu\\menutemplate.{0}.xml";

        internal const string TestTemplatesInternalPath = "\\Client\\@internal\\templates\\{0}";
        internal const string TestSWDBTemplatesInternalPath = "\\Client\\@internal\\templates\\swdb\\{0}";

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




        public static string GetTemplateInternalPath(string resource, bool isSWDB) {

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (resource.StartsWith("@")) {
                resource = resource.Substring(1);
                if (ApplicationConfiguration.IsUnitTest) {
                    return @"" + baseDirectory + (isSWDB ? TestSWDBTemplatesInternalPath.Fmt(resource): TestTemplatesInternalPath.Fmt(resource));
                }
                return @"" + baseDirectory + (isSWDB ? TemplatesSWDBInternalPath.Fmt(resource) : TemplatesInternalPath.Fmt(resource));
            }
            if (ApplicationConfiguration.IsUnitTest) {
                return @"" + baseDirectory + TestMetadataPath.Fmt(ApplicationConfiguration.ClientName) + resource;
            }

            return @"" + baseDirectory + ClientMetadataPattern.Fmt(ApplicationConfiguration.ClientName) + resource;
        }

        public static string GetMenuTemplatePath(ClientPlatform platform) {
            if (ApplicationConfiguration.IsUnitTest) {
                return AppDomain.CurrentDomain.BaseDirectory + TestMenuTemplatesInternalPath.Fmt(platform.ToString().ToLower());
            }
            return AppDomain.CurrentDomain.BaseDirectory + MenuTemplatesInternalPath.Fmt(platform.ToString().ToLower());

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


        public static StreamReader GetStream(Stream streamValidator, String path, Boolean fallbackToDefaultImpl = true) {
            return GetStreamImpl(path, streamValidator, fallbackToDefaultImpl);
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

        public static StreamReader DoGetStreamForTemplate(string templatePath, string realPath) {
            if (ApplicationConfiguration.IsUnitTest) {
                return DoGetStream(realPath);
            }

            //            if (templatePath.StartsWith("@")) {
            //                var assembly = AssemblyLocator.GetAssembly("softwrench.sw4.api");
            //                var resourceName = String.Format("softwrench.sw4.api.metadata.templates.{0}", templatePath.Substring(1));
            //                var stream = assembly.GetManifestResourceStream(resourceName);
            //                if (stream == null) {
            //                    return null;
            //                }
            //                return new StreamReader(stream);
            //            }
            return DoGetStream(realPath);
        }

        public static StreamReader GetStreamImpl(string resource, Stream streamValidator = null, bool fallbackToDefaultImpl = true) {
            if (streamValidator != null) {
                return new StreamReader(StreamUtils.CopyStream(streamValidator));
            }
            try {
                var path = GetPath(resource);
                if (File.Exists(path)) {
                    return new StreamReader(path);
                }
                if (ApplicationConfiguration.IsUnitTest) {
                    path = GetPathForUnitTestModule(resource);
                    if (File.Exists(path)) {
                        return new StreamReader(path);
                    }
                }

                if (!ApplicationConfiguration.IsLocal() && !ApplicationConfiguration.IsUnitTest && AssemblyLocator.CustomerAssemblyExists()) {
                    //we cannot call the assembly locator in unit test context
                    //under local context we shall use symbolic links in order for the refresh metadata to work
                    //TODO: do the same for non local environments, but need to explode the metadata files
                    var stream = GetStreamFromCustomerDll(resource);
                    if (stream != null) {
                        return new StreamReader(stream);
                    }
                }
                if (!fallbackToDefaultImpl) {
                    return null;
                }

                Log.InfoFormat("getting file {0} from otb default implementation", path);
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
