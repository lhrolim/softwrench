using cts.commons.persistence;
using FluentMigrator;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.DB_Migration._5._3._0 {
    [Migration(201601271130)]
    public class Migration201601271130SWWEB2020 : Migration {
        public override void Up() {
            if (!ApplicationConfiguration.IsDB2(DBType.Swdb) && Schema.Table("DASH_GRIDPANEL").Column("_limit").Exists()) {
                Rename.Column("_limit").OnTable("DASH_GRIDPANEL").To("limit_");
            }
        }

        public override void Down() {

        }
    }
}