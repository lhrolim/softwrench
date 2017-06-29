using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using FluentMigrator;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Base;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;

namespace softWrench.sW4.Web.DB_Migration.Oracle {
    public class CustomOracleProcessor : GenericProcessorBase {



        public override string DatabaseType {
            get {
                return "Oracle";
            }
        }

        public IQuoter Quoter {
            get { return new OracleQuoter(); }
        }

        public CustomOracleProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, IDbFactory factory)
            : base(connection, factory, generator, announcer, options) {
        }

        public override bool SchemaExists(string schemaName) {
            if (schemaName == null)
                throw new ArgumentNullException("schemaName");
            if (schemaName.Length == 0)
                return false;
            return this.Exists("SELECT 1 FROM ALL_USERS WHERE USERNAME = '{0}'", (object)schemaName.ToUpper());
        }

        public override bool TableExists(string schemaName, string tableName) {
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (tableName.Length == 0)
                return false;
            if (string.IsNullOrEmpty(schemaName))
                return this.Exists("SELECT 1 FROM USER_TABLES WHERE upper(TABLE_NAME) = '{0}'", (object)FormatHelper.FormatSqlEscape(tableName.ToUpper()));
            return this.Exists("SELECT 1 FROM ALL_TABLES WHERE upper(OWNER) = '{0}' AND upper(TABLE_NAME) = '{1}'", (object)schemaName.ToUpper(), (object)FormatHelper.FormatSqlEscape(tableName.ToUpper()));
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName) {
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (columnName == null)
                throw new ArgumentNullException("columnName");
            if (columnName.Length == 0 || tableName.Length == 0)
                return false;
            if (string.IsNullOrEmpty(schemaName))
                return this.Exists("SELECT 1 FROM USER_TAB_COLUMNS WHERE upper(TABLE_NAME) = '{0}' AND upper(COLUMN_NAME) = '{1}'", (object)FormatHelper.FormatSqlEscape(tableName.ToUpper()), (object)FormatHelper.FormatSqlEscape(columnName.ToUpper()));
            return this.Exists("SELECT 1 FROM ALL_TAB_COLUMNS WHERE upper(OWNER) = '{0}' AND upper(TABLE_NAME) = '{1}' AND upper(COLUMN_NAME) = '{2}'", (object)schemaName.ToUpper(), (object)FormatHelper.FormatSqlEscape(tableName.ToUpper()), (object)FormatHelper.FormatSqlEscape(columnName.ToUpper()));
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName) {
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (constraintName == null)
                throw new ArgumentNullException("constraintName");
            if (constraintName.Length == 0)
                return false;
            if (string.IsNullOrEmpty(schemaName))
                return this.Exists("SELECT 1 FROM USER_CONSTRAINTS WHERE upper(CONSTRAINT_NAME) = '{0}'", (object)FormatHelper.FormatSqlEscape(constraintName.ToUpper()));
            return this.Exists("SELECT 1 FROM ALL_CONSTRAINTS WHERE upper(OWNER) = '{0}' AND upper(CONSTRAINT_NAME) = '{1}'", (object)schemaName.ToUpper(), (object)FormatHelper.FormatSqlEscape(constraintName.ToUpper()));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName) {
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (indexName == null)
                throw new ArgumentNullException("indexName");
            if (indexName.Length == 0)
                return false;
            if (string.IsNullOrEmpty(schemaName))
                return this.Exists("SELECT 1 FROM USER_INDEXES WHERE upper(INDEX_NAME) = '{0}'", (object)FormatHelper.FormatSqlEscape(indexName.ToUpper()));
            return this.Exists("SELECT 1 FROM ALL_INDEXES WHERE upper(OWNER) = '{0}' AND upper(INDEX_NAME) = '{1}'", (object)schemaName.ToUpper(), (object)FormatHelper.FormatSqlEscape(indexName.ToUpper()));
        }

        public override bool SequenceExists(string schemaName, string sequenceName) {
            return false;
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue) {
            return false;
        }

        public override void Execute(string template, params object[] args) {
            this.Process(string.Format(template, args));
        }

        public override bool Exists(string template, params object[] args) {
            if (template == null)
                throw new ArgumentNullException("template");
            this.EnsureConnectionIsOpen();
            this.Announcer.Sql(string.Format(template, args));
            using (IDbCommand command = this.Factory.CreateCommand(string.Format(template, args), this.Connection)) {
                using (IDataReader dataReader = command.ExecuteReader())
                    return dataReader.Read();
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName) {
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (string.IsNullOrEmpty(schemaName))
                return this.Read("SELECT * FROM {0}", (object)this.Quoter.QuoteTableName(tableName));
            return this.Read("SELECT * FROM {0}.{1}", (object)this.Quoter.QuoteSchemaName(schemaName), (object)this.Quoter.QuoteTableName(tableName));
        }

        public override DataSet Read(string template, params object[] args) {
            if (template == null)
                throw new ArgumentNullException("template");
            this.EnsureConnectionIsOpen();
            DataSet dataSet = new DataSet();
            using (IDbCommand command = this.Factory.CreateCommand(string.Format(template, args), this.Connection)) {
                this.Factory.CreateDataAdapter(command).Fill(dataSet);
                return dataSet;
            }
        }

        public override void Process(PerformDBOperationExpression expression) {
            this.EnsureConnectionIsOpen();
            if (expression.Operation == null)
                return;
            expression.Operation(this.Connection, (IDbTransaction)null);
        }

        protected override void Process(string sql) {
            this.Announcer.Sql(sql);
            if (this.Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;
            this.EnsureConnectionIsOpen();
            foreach (string commandText in ((IEnumerable<string>)Regex.Split(sql, "^\\s*;\\s*$", RegexOptions.Multiline)).Select<string, string>((Func<string, string>)(x => x.Trim())).Where<string>((Func<string, bool>)(x => !string.IsNullOrEmpty(x)))) {
                using (IDbCommand command = this.Factory.CreateCommand(commandText, this.Connection))
                    command.ExecuteNonQuery();
            }
        }




    }
}