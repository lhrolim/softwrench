using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Db2Generator = FluentMigratorDB2Port.FluentMigrator.Runner.Generator.Db2Generator;

namespace FluentMigratorDB2Port.FluentMigrator.Runner.Processors.DB2
{
    public class Db2WindowsProcessorFactory : MigrationProcessorFactory
    {
        #region Methods

        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new Db2WindowsDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new Db2WindowsProcessor(connection, new Db2Generator(), announcer, options, factory);
        }

        #endregion Methods
    }
}