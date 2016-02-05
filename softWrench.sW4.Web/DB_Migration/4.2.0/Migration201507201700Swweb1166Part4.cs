using cts.commons.persistence;
using FluentMigrator;
using softWrench.sW4.Extension;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.DB_Migration._4._2._0 {
    [Migration(201507201700)]
    public class Migration201507201700Swweb1166Part4 : Migration {
        public override void Up() {
            // db2 a limitation preventing a change from varchar to clob
            if (ApplicationConfiguration.IsDB2(DBType.Swdb)) {
                Create.Column("TEMP_Message").OnTable("PROB_PROBLEM").AsClob().Nullable();
                Execute.Sql("UPDATE PROB_PROBLEM SET TEMP_Message=CAST(Message AS CLOB)");
                Delete.Column("Message").FromTable("PROB_PROBLEM");
                Rename.Column("TEMP_Message").OnTable("PROB_PROBLEM").To("Message");
            } else {
                Alter.Column("Message").OnTable("PROB_PROBLEM").AsClob();
            }
        }

        public override void Down() {
        }
    }
}