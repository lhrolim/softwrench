using System.Collections.Generic;
using NAnt.Core;

namespace softWrench.sw4.nant.classes {
    public class LogTimeCommons {
        public static Dictionary<string, long> StartTimes = new Dictionary<string, long>();

        public static bool SkipLog(Project project) {
            if (!project.Properties.Contains("logtime")) {
                return true;
            }
            var logTime = project.Properties["logtime"];
            return logTime != "true";
        }
    }
}
