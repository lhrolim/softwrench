using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Processors;

namespace softWrench.sW4.Web.DB_Migration.DB2_FluentMigrator {

    public class Db2ProcessorFactory : MigrationProcessorFactory {
        #region Methods

        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options) {
            var factory = new Db2DbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new Db2Processor(connection, new Db2Generator(), announcer, options, factory);
        }

        #endregion Methods
    }
}
