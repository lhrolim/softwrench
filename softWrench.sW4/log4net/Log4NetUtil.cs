﻿using System;
using cts.commons.portable.Util;
using cts.commons.Util;
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

        public static void InitDefaultLog() {
            XmlConfigurator.Configure();
        }

        public static void ConfigureDevLogging() {

            var swFolder = EnvironmentUtil.GetLocalSWFolder();

            //this would mean the folder was already specified via a custom environment property
            var needsFolderReplacement = !swFolder.EqualsIc("c:\\softwrench\\");
            var isDevOrQa = ApplicationConfiguration.IsDev() || ApplicationConfiguration.IsQA();
            if (ApplicationConfiguration.IsLocal() && !needsFolderReplacement) {
                //on local running instances, let´s not handle the logs at all
                return;
            }

            LoggingUtil.DefaultLog.InfoFormat("init custom log system for client {0}", ApplicationConfiguration.ClientName);


            var appenders = LogManager.GetRepository().GetAppenders();
            foreach (var appender in appenders) {
                var rollingFileAppender = appender as RollingFileAppender;
                if (rollingFileAppender == null) {
                    continue;
                }
                rollingFileAppender.MaxSizeRollBackups = ApplicationConfiguration.IsLocal() ? 1 : 3;
                if (isDevOrQa && !ApplicationConfiguration.IsLocal()) {
                    if (ApplicationConfiguration.IsDevPR()) {
                        rollingFileAppender.File = rollingFileAppender.File.Replace("\\logs\\",
                            "\\logs\\{0}\\".Fmt(ApplicationConfiguration.Profile));
                    } else {
                        rollingFileAppender.File = rollingFileAppender.File.Replace("\\logs\\",
                        "\\logs\\{0}\\".Fmt(ApplicationConfiguration.ClientName));
                    }

                } else if (!isDevOrQa) {
                    //disabling queries in production
                    ChangeLevel("MAXIMO.SQL", "WARN", null);
                    ChangeLevel("SWDB.SQL", "WARN", null);
                }
                if (needsFolderReplacement) {
                    //this is the value that was registered on the web.config file
                    rollingFileAppender.File = rollingFileAppender.File.Replace("C:\\softwrench\\", swFolder);
                }
                rollingFileAppender.ActivateOptions();
            }

            LoggingUtil.DefaultLog.InfoFormat("finish log system setup for client {0}", ApplicationConfiguration.ClientName);
        }

    }
}
