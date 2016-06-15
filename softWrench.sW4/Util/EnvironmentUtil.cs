using System;
using System.Web.Hosting;
using cts.commons.portable.Util;
using JetBrains.Annotations;

namespace softWrench.sW4.Util {
    public class EnvironmentUtil {

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




        public static string GetLocalSWFolder() {
            var softHomeVar = Environment.GetEnvironmentVariable("SOFTWRENCH_HOME");
            if (softHomeVar != null) {
                return softHomeVar + "\\";
            }
            var systemDirectory = Environment.SystemDirectory;
            var drive = systemDirectory.Substring(0, systemDirectory.IndexOf("\\", StringComparison.Ordinal));
            return "{0}\\softwrench\\".Fmt(drive);
        }

    }
}
