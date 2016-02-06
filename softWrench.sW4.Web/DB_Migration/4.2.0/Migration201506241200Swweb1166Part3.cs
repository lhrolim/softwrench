using cts.commons.persistence;
using FluentMigrator;
using softWrench.sW4.Extension;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.DB_Migration._4._2._0 {
    [Migration(201506241200)]
    public class Migration201506241200Swweb1166Part3 : Migration {

        public override void Up() {
            // db2 a limitation preventing a change from varchar to clob
            if (ApplicationConfiguration.IsDB2(DBType.Swdb)) {
                Create.Column("TEMP_StackTrace").OnTable("PROB_PROBLEM").AsClob().Nullable();
                Execute.Sql("UPDATE PROB_PROBLEM SET TEMP_StackTrace=CAST(StackTrace AS CLOB)");
                Delete.Column("StackTrace").FromTable("PROB_PROBLEM");
                Rename.Column("TEMP_StackTrace").OnTable("PROB_PROBLEM").To("StackTrace");
            } else {
                Alter.Column("StackTrace").OnTable("PROB_PROBLEM").AsClob();
            }
        }

        public override void Down() {
        }
    }
}