using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Util {


    public class Log4NetUtil {

        private const string ConnStringTemplate = "data source={0};initial catalog={1};integrated security=false;persist security info=True;User ID={2};Password={3}";

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

        public static void ConfigureAdoNetAppender() {

            var swdbUrl = MetadataProvider.GlobalProperties.GlobalProperty("swdb_url");
            //Data Source=localhost;Initial Catalog=swdb;User Id=swdb;password=pIGmkLPTJF6T;
          


            var items = swdbUrl.Split(';');

            List<string> values = new List<string>();

            foreach (var item in items) {
                if (item.Contains("="))
                    values.Add(item.Split('=')[1]);
            }

            var appenders = LogManager.GetRepository().GetAppenders();
            foreach (var appender in appenders) {
                var adoNetAppender = appender as AdoNetAppender;
                if (adoNetAppender == null) {
                    continue;
                }
                if (ApplicationConfiguration.IsLocal())
                {
                    adoNetAppender.ConnectionString =
                        "data source=localhost;initial catalog=swdb_hapag;integrated security=false;persist security info=True;User ID=sw;Password=sw";
                } else {
                    adoNetAppender.ConnectionString = ConnStringTemplate.Fmt(values[0], values[1], values[2], values[3]);
                }

                adoNetAppender.ActivateOptions();
            }

        }

    }
}