using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._4._4._1 {
    [Migration(201507231230)]
    public class Migration201507231230SWWEB1275 : FluentMigrator.Migration {
        public override void Up() {
            Create.Table("EMAIL_HISTORY")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("UserID").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("EmailAddress").AsString(MigrationUtil.StringMedium).NotNullable();
        }

        public override void Down() {
            Delete.Table("EMAIL_HISTORY");
        }
    }
}