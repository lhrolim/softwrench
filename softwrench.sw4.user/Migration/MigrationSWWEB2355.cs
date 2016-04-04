using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.user.Migration {
    [Migration(201604042009)]
    public class MigrationSwweb2355 : FluentMigrator.Migration {

        public override void Up() {
            Alter.Table("SW_USER2").AlterColumn("isactive").AsBoolean().Nullable();
            Update.Table("SW_USER2").Set(new {isactive=true}).AllRows();
        }

        public override void Down() {
        }
    }
}
