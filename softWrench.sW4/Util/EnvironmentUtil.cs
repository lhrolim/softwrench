using System;
using System.IO;
using System.Web.Hosting;
using cts.commons.portable.Util;
using JetBrains.Annotations;

namespace softWrench.sW4.Util {
    public class EnvironmentUtil {

        private static string cachedLocalSWFolder = null;

        [CanBeNull]
        public static string GetIISCustomerName() {
            var applicationPath = HostingEnvironment.ApplicationVirtualPath;
            if (!ApplicationConfiguration.IsDev()) {
                return null;
            }

            if (applicationPath != null && applicationPath.StartsWith("/") && !ApplicationConfiguration.IsDevPR()) {
                return applicationPath.Substring(1);
            }
            return null;
        }




        // ReSharper disable once InconsistentNaming
        public static string GetLocalSWFolder() {
            if (cachedLocalSWFolder != null) {
                return cachedLocalSWFolder;
            }
            cachedLocalSWFolder = DoGetLocalSWFolder();
            return cachedLocalSWFolder;
        }

        private static string DoGetLocalSWFolder() {
            var clientName = ApplicationConfiguration.ClientName;
            var softHomeVar = Environment.GetEnvironmentVariable("SOFTWRENCH_HOME");
            if (softHomeVar != null) {
                return softHomeVar + "\\";
            }
            var systemDirectory = Environment.SystemDirectory;
            var drive = systemDirectory.Substring(0, systemDirectory.IndexOf("\\", StringComparison.Ordinal));
            var baseFallBackPath = "{0}\\softwrench\\".Fmt(drive);
            if (Directory.Exists(baseFallBackPath + clientName)) {
                // https://controltechnologysolutions.atlassian.net/browse/SWWEB-2788
                return baseFallBackPath + clientName + "\\";
            }
            return baseFallBackPath;
        }
    }
}
