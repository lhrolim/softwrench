using System.Data;

using FluentMigrator.Runner.Generators.Base;

namespace softWrench.sW4.Web.DB_Migration.DB2_FluentMigrator {
    

    internal class Db2TypeMap : TypeMapBase {
        #region Methods

        /// <summary>
        /// The setup type maps.
        /// </summary>
        protected override void SetupTypeMaps() {
            this.SetTypeMap(DbType.AnsiStringFixedLength, "CHARACTER(255)");
            this.SetTypeMap(DbType.AnsiStringFixedLength, "CHARACTER($size)", 255);
            this.SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
            this.SetTypeMap(DbType.AnsiString, "VARCHAR($size)", 32704);
            this.SetTypeMap(DbType.AnsiString, "CLOB(1048576)");
            this.SetTypeMap(DbType.AnsiString, "CLOB($size)", int.MaxValue);
            this.SetTypeMap(DbType.Binary, "BINARY(255)");
            this.SetTypeMap(DbType.Binary, "BINARY($size)", 255);
            this.SetTypeMap(DbType.Binary, "VARBINARY(8000)");
            this.SetTypeMap(DbType.Binary, "VARBINARY($size)", 32704);
            this.SetTypeMap(DbType.Binary, "BLOB(1048576)");
            this.SetTypeMap(DbType.Binary, "BLOB($size)", 2147483647);
            this.SetTypeMap(DbType.Boolean, "CHAR(1)");
            this.SetTypeMap(DbType.Byte, "SMALLINT");
            this.SetTypeMap(DbType.Time, "TIME");
            this.SetTypeMap(DbType.Date, "DATE");
            this.SetTypeMap(DbType.DateTime, "TIMESTAMP");
            this.SetTypeMap(DbType.Decimal, "NUMERIC(19,5)");
            this.SetTypeMap(DbType.Decimal, "NUMERIC($size,$precision)", 31);
            this.SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
            this.SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", 31);
            this.SetTypeMap(DbType.Double, "DOUBLE");
            this.SetTypeMap(DbType.Int16, "SMALLINT");
            this.SetTypeMap(DbType.Int32, "INT");
            this.SetTypeMap(DbType.Int32, "INTEGER");
            this.SetTypeMap(DbType.Int64, "BIGINT");
            this.SetTypeMap(DbType.Single, "REAL");
            this.SetTypeMap(DbType.Single, "DECFLOAT", 34);
            this.SetTypeMap(DbType.StringFixedLength, "CHARACTER(255)");
            this.SetTypeMap(DbType.StringFixedLength, "CHARACTER($size)", 255);
            this.SetTypeMap(DbType.String, "VARCHAR(255)");
            this.SetTypeMap(DbType.String, "VARCHAR($size)", 255);
            this.SetTypeMap(DbType.String, "CLOB($size)", int.MaxValue);
            this.SetTypeMap(DbType.Xml, "XML");
        }

        #endregion
    }
}