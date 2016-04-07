using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.user.Migration {
    [Migration(201604061830)]
    public class MigrationSwweb2356 : FluentMigrator.Migration {

        public override void Up() {
            Create.UniqueConstraint("SW_MAXPERSON_UQ").OnTable("SW_USER2").Column("MAXIMOPERSONID");
        }

        public override void Down() {
        }
    }
}
