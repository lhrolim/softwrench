using cts.commons.persistence;
using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201405191200)]
    public class _20140519_SWWEB_115 : FluentMigrator.Migration {
        public override void Up() {
            //            Alter.Table("sw_user2").AlterColumn("phone").AsString(MigrationUtil.StringMedium).Nullable();
            if (ApplicationConfiguration.IsOracle(DBType.Swdb)) {
                Alter.Table("sw_user2").AlterColumn("username").AsString(MigrationUtil.StringMedium);
            } else {
                Alter.Table("sw_user2").AlterColumn("username").AsString(MigrationUtil.StringMedium).NotNullable();
            }



        }

        public override void Down() {
        }
    }
}