using System;
using cts.commons.portable.Util;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;
using softWrench.sW4.Util;

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

        public static void ConfigureLogging() {
            XmlConfigurator.Configure();

            if (!ApplicationConfiguration.IsDev() && !ApplicationConfiguration.IsLocal()) {
                return;
            }

            if (ApplicationConfiguration.IsDev()) {
                var appenders = LogManager.GetRepository().GetAppenders();
                foreach (var appender in appenders) {
                    var rollingFileAppender = appender as RollingFileAppender;
                    if (rollingFileAppender == null) {
                        continue;
                    }
                    rollingFileAppender.MaxSizeRollBackups = ApplicationConfiguration.IsLocal() ? 1 : 3;
                    rollingFileAppender.File = rollingFileAppender.File.Replace("\\sw4\\", "\\sw4\\{0}\\".Fmt(ApplicationConfiguration.ClientName));
                    rollingFileAppender.ActivateOptions();
                }
            } else {
                ChangeLevel("MAXIMO.SQL", "WARN", null);
                ChangeLevel("SWDB.SQL", "WARN", null);
            }

        }


    }
}
