using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.dashboard.classes.model.migration {

    [Migration(201603230415)]
    public class Migration20160323Swweb2200 : Migration {

        public override void Up() {
            Delete.Column("layout").FromTable("DASH_DASHBOARD");

            Create.Column("size").OnTable("DASH_BASEPANEL").AsInt32().NotNullable();
        }

        public override void Down() {
            Delete.Column("size").FromTable("DASH_BASEPANEL");

            Create.Column("layout").OnTable("DASH_DASHBOARD").AsString(MigrationUtil.StringMedium).NotNullable();
        }
    }
}
