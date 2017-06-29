using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;

namespace softWrench.sW4.Web.DB_Migration.Oracle {

    public class CustomOracleProcessorFactory : OracleProcessorFactory {

        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options) {
            OracleDbFactory oracleDbFactory = new OracleDbFactory();
            return (IMigrationProcessor)new CustomOracleProcessor(oracleDbFactory.CreateConnection(connectionString), (IMigrationGenerator)new CustomOracleGenerator(this.Quoted(options.ProviderSwitches)), announcer, options, (IDbFactory)oracleDbFactory);
        }

        private bool Quoted(string options) {
            if (!string.IsNullOrEmpty(options))
                return options.ToUpper().Contains("QUOTEDIDENTIFIERS=TRUE");
            return false;
        }

    }
}