using System;
using System.Diagnostics;
using System.Reflection;
using cts.commons.persistence;
using cts.commons.persistence.Util;
using cts.commons.Util;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.MySql;
using log4net;
using softWrench.sW4.Util;
using softWrench.sW4.Web.DB_Migration.DB2_FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration {
    public class MigratorExecutor {

        private static readonly ILog Log = LogManager.GetLogger(typeof(MigratorExecutor));

        readonly string _connectionString;
        private readonly string _serverType;

        public MigratorExecutor(string connectionKey) {
            var connectionStringSettings = ApplicationConfiguration.DBConnection(DBType.Swdb);
            _connectionString = connectionStringSettings.ConnectionString;
            var mssqlServer = ApplicationConfiguration.IsMSSQL(DBType.Swdb);
            if (mssqlServer) {
                _serverType = "mssql";
            } else {
                var db2Server = ApplicationConfiguration.IsDB2(DBType.Swdb);
                _serverType = db2Server ? "db2" : "mysql";
            }
        }

        private class MigrationOptions : IMigrationProcessorOptions {
            public bool PreviewOnly { get; set; }
            public int Timeout { get; set; }
            public string ProviderSwitches { get; private set; }
        }

        public void Migrate(Action<IMigrationRunner> runnerAction) {
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
            var migratorAssemblies = AssemblyLocator.GetMigratorAssemblies();
            runner.MigrationLoader = new MultiAssemblyMigrationLoader(runner.Conventions, migratorAssemblies, migrationContext.Namespace, migrationContext.NestedNamespaces, migrationContext.Tags);
            runner.MigrateUp(true);

            runnerAction(runner);
            Log.Info(String.Format("Migration execution finished in {0}", LoggingUtil.MsDelta(before)));
        }

        private MigrationProcessorFactory GetFactory() {
            if (_serverType == "mssql") {
                return new FluentMigrator.Runner.Processors.SqlServer.SqlServerProcessorFactory();
            }
            if (_serverType == "db2") {
                return new Db2ProcessorFactory();
            }

            return new MySqlProcessorFactory();
        }
    }
}
