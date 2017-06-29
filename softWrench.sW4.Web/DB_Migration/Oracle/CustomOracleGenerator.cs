using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Helpers;

namespace softWrench.sW4.Web.DB_Migration.Oracle {

    public class CustomOracleGenerator : GenericGenerator {


        public override string DropTable {
            get { return "DROP TABLE {0}"; }
        }

        public override string AddColumn {
            get { return "ALTER TABLE {0} ADD {1}"; }
        }

        public override string AlterColumn {
            get { return "ALTER TABLE {0} MODIFY {1}"; }
        }

        public override string RenameTable {
            get { return "ALTER TABLE {0} RENAME TO {1}"; }
        }

        public override string InsertData {
            get { return "INTO {0} ({1}) VALUES ({2})"; }
        }

        public CustomOracleGenerator()
                : base((IColumn) new CustomOracleColumn((IQuoter) new OracleQuoter()), (IQuoter) new OracleQuoter(),
                    (IDescriptionGenerator) new OracleDescriptionGenerator())
            {
        }

        public CustomOracleGenerator(bool useQuotedIdentifiers)
                : base((IColumn) new CustomOracleColumn(CustomOracleGenerator.GetQuoter(useQuotedIdentifiers)),
                    CustomOracleGenerator.GetQuoter(useQuotedIdentifiers),
                    (IDescriptionGenerator) new OracleDescriptionGenerator())
            {
        }

        private static IQuoter GetQuoter(bool useQuotedIdentifiers) {
            if (!useQuotedIdentifiers)
                return (IQuoter)new OracleQuoter();
            return (IQuoter)new OracleQuoterQuotedIdentifier();
        }

        public override string Generate(DeleteTableExpression expression) {
            return string.Format(this.DropTable,
                (object)this.ExpandTableName(this.Quoter.QuoteTableName(expression.SchemaName),
                    this.Quoter.QuoteTableName(expression.TableName)));
        }

        public override string Generate(CreateSequenceExpression expression) {
            StringBuilder stringBuilder = new StringBuilder(string.Format("CREATE SEQUENCE "));
            SequenceDefinition sequence = expression.Sequence;
            if (string.IsNullOrEmpty(sequence.SchemaName))
                stringBuilder.AppendFormat(this.Quoter.QuoteSequenceName(sequence.Name));
            else
                stringBuilder.AppendFormat("{0}.{1}", (object)this.Quoter.QuoteSchemaName(sequence.SchemaName),
                    (object)this.Quoter.QuoteSequenceName(sequence.Name));
            if (sequence.Increment.HasValue)
                stringBuilder.AppendFormat(" INCREMENT BY {0}", (object)sequence.Increment);
            if (sequence.MinValue.HasValue)
                stringBuilder.AppendFormat(" MINVALUE {0}", (object)sequence.MinValue);
            if (sequence.MaxValue.HasValue)
                stringBuilder.AppendFormat(" MAXVALUE {0}", (object)sequence.MaxValue);
            if (sequence.StartWith.HasValue)
                stringBuilder.AppendFormat(" START WITH {0}", (object)sequence.StartWith);
            if (sequence.Cache.HasValue)
                stringBuilder.AppendFormat(" CACHE {0}", (object)sequence.Cache);
            if (sequence.Cycle)
                stringBuilder.Append(" CYCLE");
            return stringBuilder.ToString();
        }

        private string ExpandTableName(string schema, string table) {
            if (!string.IsNullOrEmpty(schema))
                return schema + "." + table;
            return table;
        }

        private string innerGenerate(CreateTableExpression expression) {
            string str = this.Quoter.QuoteTableName(expression.TableName);
            return string.Format("CREATE TABLE {0} ({1})",
                (object)this.ExpandTableName(this.Quoter.QuoteSchemaName(expression.SchemaName), str),
                (object)this.Column.Generate((IEnumerable<ColumnDefinition>)expression.Columns, str));
        }

        public override string Generate(CreateTableExpression expression) {
            IEnumerable<string> descriptionStatements =
                this.DescriptionGenerator.GenerateDescriptionStatements(expression);
            string[] strArray = descriptionStatements as string[] ?? descriptionStatements.ToArray<string>();
            if (!((IEnumerable<string>)strArray).Any<string>())
                return this.innerGenerate(expression);
            StringBuilder stringBuilder =
                new StringBuilder(this.WrapStatementInExecuteImmediateBlock(this.innerGenerate(expression)));
            foreach (string statement in strArray) {
                if (!string.IsNullOrEmpty(statement)) {
                    string str = this.WrapStatementInExecuteImmediateBlock(statement);
                    stringBuilder.Append(str);
                }
            }
            return this.WrapInBlock(stringBuilder.ToString());
        }

        public override string Generate(AlterTableExpression expression) {
            string descriptionStatement = this.DescriptionGenerator.GenerateDescriptionStatement(expression);
            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);
            return descriptionStatement;
        }

        public override string Generate(CreateColumnExpression expression) {
            string descriptionStatement = this.DescriptionGenerator.GenerateDescriptionStatement(expression);
            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);
            StringBuilder stringBuilder =
                new StringBuilder(this.WrapStatementInExecuteImmediateBlock(base.Generate(expression)));
            stringBuilder.Append(this.WrapStatementInExecuteImmediateBlock(descriptionStatement));
            return this.WrapInBlock(stringBuilder.ToString());
        }

        public override string Generate(AlterColumnExpression expression) {
            string descriptionStatement = this.DescriptionGenerator.GenerateDescriptionStatement(expression);
            if (string.IsNullOrEmpty(descriptionStatement))
                return base.Generate(expression);
            StringBuilder stringBuilder =
                new StringBuilder(this.WrapStatementInExecuteImmediateBlock(base.Generate(expression)));
            stringBuilder.Append(this.WrapStatementInExecuteImmediateBlock(descriptionStatement));
            return this.WrapInBlock(stringBuilder.ToString());
        }

        public override string Generate(InsertDataExpression expression) {
            List<string> stringList1 = new List<string>();
            List<string> stringList2 = new List<string>();
            List<string> stringList3 = new List<string>();
            foreach (InsertionDataDefinition row in expression.Rows) {
                stringList1.Clear();
                stringList2.Clear();
                foreach (KeyValuePair<string, object> keyValuePair in (List<KeyValuePair<string, object>>)row) {
                    stringList1.Add(this.Quoter.QuoteColumnName(keyValuePair.Key));
                    stringList2.Add(this.Quoter.QuoteValue(keyValuePair.Value));
                }
                string str1 = string.Join(", ", stringList1.ToArray());
                string str2 = string.Join(", ", stringList2.ToArray());
                stringList3.Add(string.Format(this.InsertData,
                    (object)this.ExpandTableName(this.Quoter.QuoteSchemaName(expression.SchemaName),
                        this.Quoter.QuoteTableName(expression.TableName)), (object)str1, (object)str2));
            }
            return "INSERT ALL " + string.Join(" ", stringList3.ToArray()) + " SELECT 1 FROM DUAL";
        }

        public override string Generate(AlterDefaultConstraintExpression expression) {
            string alterColumn = this.AlterColumn;
            string str1 = this.Quoter.QuoteTableName(expression.TableName);
            IColumn column1 = this.Column;
            ColumnDefinition column2 = new ColumnDefinition();
            column2.ModificationType = ColumnModificationType.Alter;
            string columnName = expression.ColumnName;
            column2.Name = columnName;
            object defaultValue = expression.DefaultValue;
            column2.DefaultValue = defaultValue;
            string str2 = column1.Generate(column2);
            return string.Format(alterColumn, (object)str1, (object)str2);
        }

        public override string Generate(DeleteDefaultConstraintExpression expression) {
            return this.Generate(new AlterDefaultConstraintExpression() {
                TableName = expression.TableName,
                ColumnName = expression.ColumnName,
                DefaultValue = (object)null
            });
        }

        private string WrapStatementInExecuteImmediateBlock(string statement) {
            if (string.IsNullOrEmpty(statement))
                return string.Empty;
            return string.Format("EXECUTE IMMEDIATE '{0}';", (object)FormatHelper.FormatSqlEscape(statement));
        }

        private string WrapInBlock(string sql) {
            if (string.IsNullOrEmpty(sql))
                return string.Empty;
            return string.Format("BEGIN {0} END;", (object)sql);
        }

    }
}