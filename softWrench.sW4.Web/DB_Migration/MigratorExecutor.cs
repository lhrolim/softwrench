using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.MySql;
using log4net;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.DB_Migration {
    public class MigratorExecutor {

        private static readonly ILog Log = LogManager.GetLogger(typeof(MigratorExecutor));

        readonly string _connectionString;
        private readonly string _serverType;

        public MigratorExecutor(string connectionKey) {
            var connectionStringSettings = ApplicationConfiguration.DBConnection(ApplicationConfiguration.DBType.Swdb);
            _connectionString = connectionStringSettings.ConnectionString;
            var mssqlServer = ApplicationConfiguration.IsMSSQL(ApplicationConfiguration.DBType.Swdb);
            if (mssqlServer) {
                _serverType = "mssql";
            } else {
                var db2Server = ApplicationConfiguration.IsDB2(ApplicationConfiguration.DBType.Swdb);
                _serverType = db2Server ? "db2" : "mysql";
            }
        }

        private class MigrationOptions : IMigrationProcessorOptions {
            public bool PreviewOnly { get; set; }
            public int Timeout { get; set; }
            public string ProviderSwitches { get; private set; }
        }

        public void Migrate(Action<IMigrationRunner> runnerAction) {
            if (_serverType == "db2") {
                Log.Info(String.Format("Migrator skipped due to SWDB using DB2"));
                return;
            }
            var before = Stopwatch.StartNew();
            if (ApplicationConfiguration.IsLocal() && !ApplicationConfiguration.IsLocalHostSWDB()) {
                Log.Debug("Ignoring Migration on remoteDB");
                //avoid misconfiguration to change the schema of a remote database
                return;
            }

            var options = new MigrationOptions { PreviewOnly = false, Timeout = 0 };
            var factory = GetFactory();
            var assembly = Assembly.GetExecutingAssembly();

            //using (var announcer = new NullAnnouncer())
            var announcer = new TextWriterAnnouncer(s => System.Diagnostics.Debug.WriteLine(s));
            var migrationContext = new RunnerContext(announcer) {
            #if DEBUG
                // will create testdata
                Profile = "development"
            #endif
            };
            var processor = factory.Create(_connectionString, announcer, options);
            var runner = new MigrationRunner(assembly, migrationContext, processor);
            runnerAction(runner);
            Log.Info(String.Format("Migration execution finished in {0}", LoggingUtil.MsDelta(before)));
        }

        private MigrationProcessorFactory GetFactory() {
            if (_serverType == "mssql") {
                return new FluentMigrator.Runner.Processors.SqlServer.SqlServerProcessorFactory();
            }

            return new MySqlProcessorFactory();
        }
    }
}
