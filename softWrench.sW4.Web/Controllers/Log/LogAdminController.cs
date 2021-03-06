﻿using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;
using softWrench.sW4.Data.API;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SPF;
using softWrench.sW4.Web.SPF;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace softWrench.sW4.Web.Controllers.Log {

    public class LogAdminController : ApiController {


        [SPFRedirect("Log Administrator", "_headermenu.logadmin")]
        [HttpGet]
        public IGenericResponseResult Index() {
            var logs = GetLogs();
            var appenders = GetAppenders();
            var logModel = new LogModel(logs, appenders);

            return new GenericResponseResult<LogModel>(logModel);
        }

        private List<LogAdmin> GetLogs() {
            var logs = new List<LogAdmin>();

            log4net.Repository.ILoggerRepository[] repositories = log4net.LogManager.GetAllRepositories();

            //Configure all loggers to be at the debug level.
            foreach (log4net.Repository.ILoggerRepository repository in repositories) {
                var hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
                log4net.Core.ILogger[] loggers = hier.GetCurrentLoggers();
                foreach (log4net.Core.ILogger logger in loggers) {
                    var level = ((log4net.Repository.Hierarchy.Logger)logger).Level;
                    logs.Add(new LogAdmin(logger.Name, level == null ? "none" : level.DisplayName.ToLower()));
                    //                    ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap["DEBUG"];
                }
            }
            return logs;
        }

        private List<Appender> GetAppenders() {
            var apperderList = new List<Appender> { new Appender("-- Select One --", null) };
            var appenders = log4net.LogManager.GetRepository().GetAppenders();

            foreach (var appender in appenders) {
                var rollingFileAppender = appender as RollingFileAppender;
                if (rollingFileAppender != null) {
                    apperderList.Add(new Appender(appender.Name, rollingFileAppender.File));
                }
            }
            return apperderList;
        }

        [HttpGet]
        public IGenericResponseResult GetAppenderTxtContent(string value) {
            var content = string.Empty;
            if (value == null) {
                return new GenericResponseResult<string>(content);
            }
            using (var fileStream = new FileStream(value, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(fileStream)) {
                content = streamReader.ReadToEnd();
            }
            return new GenericResponseResult<string>(content);
        }

        [HttpPost]
        public IGenericResponseResult ChangeLevel(string logName, string newLevel, string pattern) {
            var repositories = log4net.LogManager.GetAllRepositories();
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
                foreach (log4net.Core.ILogger logger in loggers) {
                    var exactLog = !logName.NullOrEmpty() && logger.Name.Equals(logName, StringComparison.CurrentCultureIgnoreCase);
                    var patternMatch = logName.NullOrEmpty() && logger.Name.ToLower().Contains(pattern.ToLower());
                    if (exactLog || patternMatch) {
                        ((Logger)logger).Level = newLevelToSet;
                    }
                }
            }
            return Index();
        }

        public class LogModel {
            public LogModel(List<LogAdmin> logs, List<Appender> appenders) {
                Logs = logs;
                Appenders = appenders;
            }
            public List<LogAdmin> Logs { get; set; }
            public List<Appender> Appenders { get; set; }
        }

        public class Appender {
            public Appender(string name, string value) {
                Name = name;
                Value = value;
            }
            public String Name { get; set; }
            public String Value { get; set; }
        }

        public class LogAdmin {
            public LogAdmin(string name, string level) {
                Name = name;
                Level = level;
            }
            public String Name { get; set; }
            public String Level { get; set; }
        }

    }
}