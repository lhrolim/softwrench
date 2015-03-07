using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SPF;

namespace softWrench.sW4.log4net {
    public class Log4NetUtil {


        public static void ChangeLevel(string logName, string newLevel, string pattern) {
            var repositories = LogManager.GetAllRepositories();
            if (pattern == null) {
                pattern = "";
            }

            //Configure all loggers to be at the debug level.
            foreach (var repository in repositories) {
                var hier = (Hierarchy)repository;
                var newLevelToSet = hier.LevelMap[newLevel.ToUpper()];
                if (newLevel.Equals("none")) {
                    newLevelToSet = null;
                }
                var loggers = hier.GetCurrentLoggers();
                foreach (ILogger logger in loggers) {
                    var exactLog = !logName.NullOrEmpty() && logger.Name.Equals(logName, StringComparison.CurrentCultureIgnoreCase);
                    var patternMatch = logName.NullOrEmpty() && logger.Name.ToLower().Contains(pattern.ToLower());
                    if (exactLog || patternMatch) {
                        ((Logger)logger).Level = newLevelToSet;
                    }
                }
            }
        }


    }
}
