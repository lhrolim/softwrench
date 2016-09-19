using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;
using FluentMigrator.Model;

namespace softWrench.sW4.Web.DB_Migration.DB2_FluentMigrator {


    internal class Db2Column : ColumnBase {
        #region Constructors

        public Db2Column()
            : base(new Db2TypeMap(), new Db2Quoter()) {
            this.ClauseOrder = new List<Func<ColumnDefinition, string>> { FormatString, FormatType, this.FormatNullable, this.FormatDefaultValue, this.FormatIdentity };
            this.AlterClauseOrder = new List<Func<ColumnDefinition, string>> { FormatType, this.FormatNullableWithDrop, this.FormatDefaultValue, this.FormatIdentity };
        }

        #endregion Constructors

        #region Properties

        public List<Func<ColumnDefinition, string>> AlterClauseOrder {
            get; set;
        }

        #endregion Properties

        #region Methods

        public string FormatAlterDefaultValue(string column, object defaultValue) {
            return defaultValue is SystemMethods
                ? this.FormatSystemMethods((SystemMethods)defaultValue)
                : Quoter.QuoteValue(defaultValue);
        }

        public string GenerateAlterColumn(ColumnDefinition column, string formattedTable) {
            if (column.IsIdentity) {
                throw new NotSupportedException("Altering an identity column is not supported.");
            }

            var formattedColumn = Quoter.QuoteColumnName(column.Name);
            var alterTable = string.Format("ALTER TABLE {0} ", formattedTable);
            var alterClauses = AlterClauseOrder.Aggregate(new StringBuilder(), (acc, newRow) => {
                var clause = newRow(column);
                if (acc.Length == 0) {
                    var dataTypeClause = string.Format("ALTER COLUMN {0} SET DATA TYPE {1}", formattedColumn,
                        newRow(column));
                    acc.Append(alterTable).Append(dataTypeClause);
                } else if (!string.IsNullOrEmpty(clause)) {
                    var dropNull = clause.StartsWith("DROP");
                    var innerClause = dropNull ? clause : "SET " + clause;
                    acc.Append("; ");
                    acc.Append(alterTable).AppendFormat(" ALTER COLUMN {0} {1}", formattedColumn, innerClause);
                    if (dropNull) {
                        acc.Append("; ").AppendFormat(" Call Sysproc.admin_cmd ('reorg Table {0}')", formattedTable);
                    }
                }

                return acc;
            });

            return alterClauses.ToString();
        }

        //protected virtual string FormatCCSID(ColumnDefinition column) {
        //    if (column.Type == null) {
        //        return string.Empty;
        //    }

        //    var dbType = (DbType)column.Type;

        //    if (DbType.String.Equals(dbType) || DbType.StringFixedLength.Equals(dbType)) {
        //        // Force UTF-16 on double-byte character types.
        //        return "CCSID 1200";
        //    }

        //    return string.Empty;
        //}

        protected override string FormatDefaultValue(ColumnDefinition column) {
            var isCreate = column.GetAdditionalFeature<bool>("IsCreateColumn", false);

            if (isCreate && (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue)) {
                return "DEFAULT";
            }

            if (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue) {
                return string.Empty;
            }

            // see if this is for a system method
            if (!(column.DefaultValue is SystemMethods)) {
                return "DEFAULT " + this.Quoter.QuoteValue(column.DefaultValue);
            }

            var method = this.FormatSystemMethods((SystemMethods)column.DefaultValue);
            if (string.IsNullOrEmpty(method)) {
                return string.Empty;
            }

            return "DEFAULT " + method;
        }

        protected override string FormatIdentity(ColumnDefinition column) {
            return column.IsIdentity ? "GENERATED ALWAYS AS IDENTITY" : string.Empty;
        }

        protected override string FormatNullable(ColumnDefinition column) {
            if (column.IsNullable.HasValue && column.IsNullable.Value) {
                return string.Empty;
            }

            return "NOT NULL";
        }

        protected string FormatNullableWithDrop(ColumnDefinition column) {
            if (column.IsNullable.HasValue && column.IsNullable.Value) {
                return "DROP NOT NULL";
            }

            return "NOT NULL";
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod) {
            switch (systemMethod) {
                case SystemMethods.CurrentUTCDateTime:
                    return "(CURRENT_TIMESTAMP - CURRENT_TIMEZONE)";
                case SystemMethods.CurrentDateTime:
                    return "CURRENT_TIMESTAMP";
                case SystemMethods.CurrentUser:
                    return "USER";
            }

            throw new NotImplementedException();
        }

        #endregion Methods
    }
}