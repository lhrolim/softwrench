using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.user.Migration {
    [Migration(201701192232)]
    public class MigrationSwweb2907 : FluentMigrator.Migration {

        public override void Up() {
            Alter.Table("SW_USER2").AddColumn("creationdate").AsDateTime().Nullable();
            Alter.Table("SW_USER2").AddColumn("creationtype").AsString(MigrationUtil.StringSmall);
        }

        public override void Down() {
        }
    }
}
