using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201410021630)]
    public class Migration201410021630SWWEB478 : FluentMigrator.Migration {
        public override void Up() {
            Create.Table("MAX_COMMREADFLAG")
                .WithIdColumn(true)
                .WithColumn("Application").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("ApplicationItemId").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("UserId").AsInt32().NotNullable()
                .WithColumn("CommlogId").AsInt64().NotNullable()
                .WithColumn("ReadFlag").AsBoolean().NotNullable();
        }

        public override void Down() {
            Delete.Table("MAX_COMMREADFLAG");
        }
    }
}