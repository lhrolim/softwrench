using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.user.Migration {
    [Migration(201604061830)]
    public class MigrationSwweb2356 : FluentMigrator.Migration {

        public override void Up()
        {
            Create.Column("systemuser").OnTable("SW_USER2").AsBoolean().WithDefaultValue(false);
            
            Update.Table("SW_USER2").Set(new { maximopersonid = "swadmin" }).Where(new { username = "swadmin" });
            Update.Table("SW_USER2").Set(new { maximopersonid = "swjobuser" }).Where(new { username = "swjobuser" });
            Update.Table("SW_USER2").Set(new {systemuser = true}).Where(new { username ="swadmin"});
            Update.Table("SW_USER2").Set(new {systemuser = true}).Where(new { username ="swjobuser"});

            Create.UniqueConstraint("SW_MAXPERSON_UQ").OnTable("SW_USER2").Column("MAXIMOPERSONID");
        }

        public override void Down() {
        }
    }
}
