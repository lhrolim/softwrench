using System;
using cts.commons.persistence;
using FluentMigrator.Builders.Alter.Column;
using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Exceptions;
using FluentMigrator.Infrastructure;
using softWrench.sW4.Util;
using MigrationContext = softwrench.sw4.api.classes.migration.MigrationContext;

namespace softWrench.sW4.Extension {

    public static class MigrationExtensions {

        /// <summary>
        /// Extends migration to add method AsClob when altering a column
        /// Determines the proper "CLOB" datatype according to the database being used.
        /// 
        /// </summary>
        /// <param name="alterColumnAsTypeOrInSchemaSyntax"></param>
        /// <returns></returns>
        public static IAlterColumnOptionSyntax AsClob(this IAlterColumnAsTypeOrInSchemaSyntax alterColumnAsTypeOrInSchemaSyntax) {
            var customClobType = CustomClobType();
            return alterColumnAsTypeOrInSchemaSyntax.AsCustom(customClobType);
        }

        /// <summary>
        /// Extends migration to add method AsClob when creating a column.
        /// Determines the proper "CLOB" datatype according to the database being used.
        /// 
        /// </summary>
        /// <param name="createTableColumnAsTypeSyntax"></param>
        /// <returns></returns>
        public static ICreateTableColumnOptionOrWithColumnSyntax AsClob(this ICreateTableColumnAsTypeSyntax createTableColumnAsTypeSyntax) {
            var customClobType = CustomClobType();
            return createTableColumnAsTypeSyntax.AsCustom(customClobType);
        }



        /// <summary>
        /// Extends migration to add method AsClob when creating a column
        /// Determines the proper "CLOB" datatype according to the database being used.
        /// 
        /// </summary>
        /// <param name="createColumnAsTypeOrInSchemaSyntax"></param>
        /// <returns></returns>
        public static ICreateColumnOptionSyntax AsClob(this ICreateColumnAsTypeOrInSchemaSyntax createColumnAsTypeOrInSchemaSyntax) {
            var customClobType = CustomClobType();
            return createColumnAsTypeOrInSchemaSyntax.AsCustom(customClobType);
        }

        /// <summary>
        /// Method to create binary tables that work cross database
        /// 
        /// </summary>
        /// <param name="tableWithColumnSyntax"></param>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public static ICreateTableColumnOptionOrWithColumnSyntax AsSwBinary(this ICreateTableColumnAsTypeSyntax tableWithColumnSyntax, bool nullable = true) {
            if (!MigrationContext.IsMySql) {
                var s = tableWithColumnSyntax.AsBinary(int.MaxValue);
                if (nullable) {
                    return s.Nullable();
                }

                return s;
            }
            return nullable ? tableWithColumnSyntax.AsBinary().Nullable() : tableWithColumnSyntax.AsBinary();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax WithIdColumn(this ICreateTableWithColumnSyntax tableWithColumnSyntax, bool use64 = false) {
            var initialSyntax = tableWithColumnSyntax.WithColumn("ID");
            var typeSyntax = use64 ? initialSyntax.AsInt64() : initialSyntax.AsInt32();
            var tableCreationSyntax = typeSyntax
                .NotNullable()
                .PrimaryKey();
            if (softwrench.sw4.api.classes.migration.MigrationContext.IsOracle) {
                //oracles does not allow identities
                return tableCreationSyntax;
            }

            return tableCreationSyntax
                .Identity();
        }

        private static string CustomClobType() {
            string customClobType = null;
            var dbType = ApplicationConfiguration.DiscoverDBMS(DBType.Swdb);
            switch (dbType) {
                case DBMS.MSSQL:
                    customClobType = "NVARCHAR(MAX)";
                    break;
                case DBMS.MYSQL:
                    customClobType = "LONGTEXT";
                    break;
                case DBMS.ORACLE:
                    customClobType = "CLOB";
                    break;
                case DBMS.DB2:
                    customClobType = "CLOB";
                    break;
                default:
                    throw new DatabaseOperationNotSupportedException(
                        String.Format("The database {0} is not currently supported for creating CLOB columns", dbType));
            }
            return customClobType;
        }



    }
}
