using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;

namespace softWrench.sW4.Util {
    public class EnvironmentUtil {

        public static string GetLocalSWFolder() {
            var softHomeVar = Environment.GetEnvironmentVariable("SOFTWRENCH_HOME");
            if (softHomeVar != null) {
                return softHomeVar + "\\properties.xml";
            }
            var systemDirectory = Environment.SystemDirectory;
            var drive = systemDirectory.Substring(0, systemDirectory.IndexOf("\\", StringComparison.Ordinal));
            return "{0}\\softwrench\\properties.xml".Fmt(drive);
        }

    }
}
