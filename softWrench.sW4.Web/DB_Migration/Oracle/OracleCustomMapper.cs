using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using FluentMigrator.Exceptions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;

namespace softWrench.sW4.Web.DB_Migration.Oracle {


    public class OracleCustomMapper : OracleTypeMap {
        protected override void SetupTypeMaps() {
            this.SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255 CHAR)");
            this.SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size CHAR)", 2000);
            this.SetTypeMap(DbType.AnsiString, "VARCHAR2(255 CHAR)");
            this.SetTypeMap(DbType.AnsiString, "VARCHAR2($size CHAR)", 4000);
            this.SetTypeMap(DbType.AnsiString, "CLOB", int.MaxValue);
            this.SetTypeMap(DbType.Binary, "RAW(2000)");
            this.SetTypeMap(DbType.Binary, "RAW($size)", 2000);
            this.SetTypeMap(DbType.Binary, "BLOB", int.MaxValue);
            this.SetTypeMap(DbType.Boolean, "NUMBER(1,0)");
            this.SetTypeMap(DbType.Byte, "NUMBER(3,0)");
            this.SetTypeMap(DbType.Currency, "NUMBER(19,4)");
            this.SetTypeMap(DbType.Date, "DATE");
            this.SetTypeMap(DbType.DateTime, "TIMESTAMP(4)");
            this.SetTypeMap(DbType.DateTimeOffset, "TIMESTAMP(4) WITH TIME ZONE");
            this.SetTypeMap(DbType.Decimal, "NUMBER(19,5)");
            this.SetTypeMap(DbType.Decimal, "NUMBER($size,$precision)", 38);
            this.SetTypeMap(DbType.Double, "DOUBLE PRECISION");
            this.SetTypeMap(DbType.Guid, "RAW(16)");
            this.SetTypeMap(DbType.Int16, "NUMBER(5,0)");
            this.SetTypeMap(DbType.Int32, "NUMBER(10,0)");
            this.SetTypeMap(DbType.Int64, "NUMBER(19,0)");
            this.SetTypeMap(DbType.Single, "FLOAT(24)");
            this.SetTypeMap(DbType.StringFixedLength, "NCHAR(255)");
            this.SetTypeMap(DbType.StringFixedLength, "NCHAR($size)", 2000);
            this.SetTypeMap(DbType.String, "NVARCHAR2(255)");
            SetTypeMap(DbType.String, "NVARCHAR2($size)", 2000);
            this.SetTypeMap(DbType.String, "NCLOB", int.MaxValue);
            this.SetTypeMap(DbType.Time, "DATE");
            this.SetTypeMap(DbType.Xml, "XMLTYPE");
            
        }
    }
    

    public abstract class CustomColumnBase : IColumn {

        private readonly ITypeMap _typeMap;
        private readonly IQuoter _quoter;

        protected IList<Func<ColumnDefinition, string>> ClauseOrder { get; set; }

        protected IQuoter Quoter {
            get {
                return this._quoter;
            }
        }

        public CustomColumnBase(ITypeMap typeMap, IQuoter quoter) {
            this._typeMap = typeMap;
            this._quoter = quoter;
            this.ClauseOrder = (IList<Func<ColumnDefinition, string>>)new List<Func<ColumnDefinition, string>>()
            {
                    new Func<ColumnDefinition, string>(this.FormatString),
                    new Func<ColumnDefinition, string>(this.FormatType),
                    new Func<ColumnDefinition, string>(this.FormatCollation),
                    new Func<ColumnDefinition, string>(this.FormatNullable),
                    new Func<ColumnDefinition, string>(this.FormatDefaultValue),
                    new Func<ColumnDefinition, string>(this.FormatPrimaryKey),
                    new Func<ColumnDefinition, string>(this.FormatIdentity)
                };
        }

        protected string GetTypeMap(DbType value, int size, int precision) {
            return this._typeMap.GetTypeMap(value, size, precision);
        }

        public virtual string FormatString(ColumnDefinition column) {
            return this._quoter.QuoteColumnName(column.Name);
        }

        protected virtual string FormatType(ColumnDefinition column) {
            if (!column.Type.HasValue)
                return column.CustomType;
            return this.GetTypeMap(column.Type.Value, column.Size, column.Precision);
        }

        public virtual string FormatNullable(ColumnDefinition column) {
            if (column.IsNullable.HasValue && column.IsNullable.Value)
                return string.Empty;
            return "NOT NULL";
        }

        public virtual string FormatDefaultValue(ColumnDefinition column) {
            if (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
                return string.Empty;
            if (!(column.DefaultValue is SystemMethods))
                return "DEFAULT " + this.Quoter.QuoteValue(column.DefaultValue);
            string str = this.FormatSystemMethods((SystemMethods)column.DefaultValue);
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return "DEFAULT " + str;
        }

        protected abstract string FormatIdentity(ColumnDefinition column);

        protected abstract string FormatSystemMethods(SystemMethods systemMethod);

        protected virtual string FormatPrimaryKey(ColumnDefinition column) {
            return string.Empty;
        }

        protected virtual string FormatCollation(ColumnDefinition column) {
            if (!string.IsNullOrEmpty(column.CollationName))
                return "COLLATE " + column.CollationName;
            return string.Empty;
        }

        public virtual string Generate(ColumnDefinition column) {
            List<string> stringList = new List<string>();
            foreach (Func<ColumnDefinition, string> func in (IEnumerable<Func<ColumnDefinition, string>>)this.ClauseOrder) {
                string str = func(column);
                if (!string.IsNullOrEmpty(str))
                    stringList.Add(str);
            }
            return string.Join(" ", stringList.ToArray());
        }

        public string Generate(IEnumerable<ColumnDefinition> columns, string tableName) {
            string str = string.Empty;
            IEnumerable<ColumnDefinition> primaryKeyColumns = columns.Where<ColumnDefinition>((Func<ColumnDefinition, bool>)(x => x.IsPrimaryKey));
            if (this.ShouldPrimaryKeysBeAddedSeparately(primaryKeyColumns)) {
                str = this.AddPrimaryKeyConstraint(tableName, primaryKeyColumns);
                foreach (ColumnDefinition column in columns)
                    column.IsPrimaryKey = false;
            }
            return string.Join(", ", columns.Select<ColumnDefinition, string>((Func<ColumnDefinition, string>)(x => this.Generate(x))).ToArray<string>()) + str;
        }

        public virtual bool ShouldPrimaryKeysBeAddedSeparately(IEnumerable<ColumnDefinition> primaryKeyColumns) {
            return primaryKeyColumns.Any<ColumnDefinition>((Func<ColumnDefinition, bool>)(x => x.IsPrimaryKey));
        }

        public virtual string AddPrimaryKeyConstraint(string tableName, IEnumerable<ColumnDefinition> primaryKeyColumns) {
            string str = string.Join(", ", primaryKeyColumns.Select<ColumnDefinition, string>((Func<ColumnDefinition, string>)(x => this.Quoter.QuoteColumnName(x.Name))).ToArray<string>());
            return string.Format(", {0}PRIMARY KEY ({1})", (object)this.GetPrimaryKeyConstraintName(primaryKeyColumns, tableName), (object)str);
        }

        protected virtual string GetPrimaryKeyConstraintName(IEnumerable<ColumnDefinition> primaryKeyColumns, string tableName) {
            string indexName = primaryKeyColumns.Select<ColumnDefinition, string>((Func<ColumnDefinition, string>)(x => x.PrimaryKeyName)).FirstOrDefault<string>();
            if (string.IsNullOrEmpty(indexName))
                return string.Empty;
            return string.Format("CONSTRAINT {0} ", (object)this.Quoter.QuoteIndexName(indexName));

        }
    }


    public class CustomOracleColumn : CustomColumnBase {
        private const int OracleObjectNameMaxLength = 30;

        public CustomOracleColumn(IQuoter quoter)
            : base((ITypeMap)new OracleCustomMapper(), quoter) {
            int index1 = this.ClauseOrder.IndexOf(new Func<ColumnDefinition, string>(((CustomColumnBase)this).FormatDefaultValue));
            int index2 = this.ClauseOrder.IndexOf(new Func<ColumnDefinition, string>(((CustomColumnBase)this).FormatNullable));
            if (index1 <= index2)
                return;
            this.ClauseOrder[index2] = new Func<ColumnDefinition, string>(((CustomColumnBase)this).FormatDefaultValue);
            this.ClauseOrder[index1] = new Func<ColumnDefinition, string>(((CustomColumnBase)this).FormatNullable);
        }

        protected override string FormatIdentity(ColumnDefinition column) {
//            if (column.IsIdentity)
//                throw new DatabaseOperationNotSupportedException("Oracle does not support identity columns. Please use a SEQUENCE instead");
            return string.Empty;
        }

        public override string FormatNullable(ColumnDefinition column) {
            if (column.ModificationType == ColumnModificationType.Create) {
                if (column.IsNullable.HasValue && column.IsNullable.Value)
                    return string.Empty;
                return "NOT NULL";
            }
            if (!column.IsNullable.HasValue)
                return string.Empty;
            return !column.IsNullable.Value ? "NOT NULL" : "NULL";
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod) {
            switch (systemMethod) {
                case SystemMethods.NewGuid:
                    return "sys_guid()";
                case SystemMethods.CurrentDateTime:
                    return "CURRENT_TIMESTAMP";
                case SystemMethods.CurrentUser:
                    return "USER";
                default:
                    throw new NotImplementedException();
            }
        }

        protected override string GetPrimaryKeyConstraintName(IEnumerable<ColumnDefinition> primaryKeyColumns, string tableName) {
            if (primaryKeyColumns == null)
                throw new ArgumentNullException("primaryKeyColumns");
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            string primaryKeyName = primaryKeyColumns.First<ColumnDefinition>().PrimaryKeyName;
            if (string.IsNullOrEmpty(primaryKeyName))
                return string.Empty;
            if (primaryKeyName.Length > 30)
                primaryKeyName = primaryKeyName.Substring(30);
//                throw new DatabaseOperationNotSupportedException(string.Format("Oracle does not support length of primary key name greater than {0} characters. Reduce length of primary key name. ({1})", (object)30, (object)primaryKeyName));
            return string.Format("CONSTRAINT {0} ", (object)this.Quoter.QuoteConstraintName(primaryKeyName));
        }
    }

}