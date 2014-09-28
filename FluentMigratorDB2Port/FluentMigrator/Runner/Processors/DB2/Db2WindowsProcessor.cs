using FluentMigrator;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Processors;
using System.Data;
using System.Linq;

namespace FluentMigratorDB2Port.FluentMigrator.Runner.Processors.DB2
{
    public class Db2WindowsProcessor : GenericProcessorBase
    {
        #region Constructors

        public Db2WindowsProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, IDbFactory factory)
            : base(connection, factory, generator, announcer, options)
        {
            this.Quoter = new Db2Quoter();
        }

        #endregion Constructors

        #region Properties

        public override string DatabaseType
        {
            get { return "IBM DB2"; }
        }

        public IQuoter Quoter
        {
            get;
            set;
        }

        public override bool SupportsTransactions
        {
            get
            {
                return true;
            }
        }

        #endregion Properties

        #region Methods

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABSCHEMA = '" + this.FormatToSafeName(schemaName) + "' AND ";

            var doesExist = this.Exists("SELECT COLNAME FROM SYSCAT.COLUMNS WHERE {0} TABNAME = '{1}' AND COLNAME='{2}'", schema, this.FormatToSafeName(tableName), this.FormatToSafeName(columnName));
            return doesExist;
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABSCHEMA = '" + this.FormatToSafeName(schemaName) + "' AND ";

            return this.Exists("SELECT CONSTNAME FROM SYSCAT.TABCONST WHERE {0} TABNAME = '{1}' AND CONSTNAME='{2}'", schema, this.FormatToSafeName(tableName), this.FormatToSafeName(constraintName));
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABSCHEMA = '" + this.FormatToSafeName(schemaName) + "' AND ";
            var defaultValueAsString = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));

            return this.Exists("SELECT DEFAULT FROM SYSCAT.COLUMNS WHERE {0} TABNAME = '{1}' AND COLNAME = '{2}' AND DEFAULT LIKE '{3}'", schema, this.FormatToSafeName(tableName), columnName.ToUpper(), defaultValueAsString);
        }

        public override void Execute(string template, params object[] args)
        {
            this.Process(string.Format(template, args));
        }

        public override bool Exists(string template, params object[] args)
        {
            this.EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(string.Format(template, args), Connection, Transaction))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            var schema = !string.IsNullOrEmpty(schemaName) ? this.Quoter.QuoteSchemaName(schemaName) + "." : string.Empty;
            var doesExist = this.Exists(
                "SELECT INDNAME FROM SYSCAT.INDEXES WHERE TABNAME = '{1}' AND INDNAME = '{2}'",
                schema,
                this.FormatToSafeName(tableName),
                this.FormatToSafeName(indexName));

            return doesExist;
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            Announcer.Say("Performing DB Operation");

            if (Options.PreviewOnly)
            {
                return;
            }

            this.EnsureConnectionIsOpen();

            if (expression.Operation != null)
            {
                expression.Operation(this.Connection, this.Transaction);
            }
        }

        public override DataSet Read(string template, params object[] args)
        {
            this.EnsureConnectionIsOpen();

            var ds = new DataSet();
            using (var command = Factory.CreateCommand(string.Format(template, args), Connection, Transaction))
            {
                var adapter = Factory.CreateDataAdapter(command);
                adapter.Fill(ds);
                return ds;
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            var schemaAndTable = !string.IsNullOrEmpty(schemaName) ? this.Quoter.QuoteSchemaName(schemaName) + "." + this.Quoter.QuoteTableName(tableName) : this.Quoter.QuoteTableName(tableName);
            return this.Read("SELECT * FROM {0}", schemaAndTable);
        }

        public override bool SchemaExists(string schemaName)
        {
            return this.Exists("SELECT SCHEMANAME FROM SYSCAT.SCHEMATA WHERE SCHEMANAME = '{0}'", this.FormatToSafeName(schemaName));
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABSCHEMA = '" + this.FormatToSafeName(schemaName) + "' AND ";

            return this.Exists("SELECT TABNAME FROM SYSCAT.TABLES WHERE {0} TABNAME = '{1}'", schema, this.FormatToSafeName(tableName));
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
            {
                return;
            }

            this.EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(sql, Connection,Transaction))
            {
                command.CommandTimeout = Options.Timeout;
                command.ExecuteNonQuery();
            }
        }

        private string FormatToSafeName(string sqlName)
        {
            var rawName = this.Quoter.UnQuote(sqlName);

            return rawName.Contains('\'') ? FormatHelper.FormatSqlEscape(rawName) : rawName.ToUpper();
        }

        #endregion Methods
    }
}