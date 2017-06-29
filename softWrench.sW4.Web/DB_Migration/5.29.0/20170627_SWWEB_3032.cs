using cts.commons.persistence.Util;
using FluentMigrator;
using softwrench.sw4.api.classes.migration;

namespace softWrench.sW4.Web.DB_Migration._5._29._0 {
    [Migration(201706271337)]

    public class Migration20170627Swweb3032 : Migration {

        public override void Up() {



            if (Schema.Table("CONF_WCCONDITION").Column("mode").Exists()) {
                Rename.Column("mode").OnTable("CONF_WCCONDITION").To("mode_");
            }


            if (Schema.Table("DASH_BASEPANEL").Column("size").Exists()) {
                Rename.Column("size").OnTable("DASH_BASEPANEL").To("size_");
            }

            if (MigrationContext.IsOracle) {
                Create.Sequence("hibernate_sequence");
            }


        }

        public override void Down() {

        }
    }
}
