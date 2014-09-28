using FluentMigrator;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {

    [Migration(201404290614)]
    public class Migration20140422Swweb112_2 : FluentMigrator.Migration {

        public override void Up() {
            Create.UniqueConstraint("SEC_uq_PG_NAME").OnTable("SEC_PERSONGROUP").Column("name");
            Create.Column("supergroup").OnTable("SEC_PERSONGROUP").AsBoolean().WithDefaultValue(false);
        }

        public override void Down() {

        }
    }
}