using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201410011621)]
    public class Migration20141001Swweb505 : FluentMigrator.Migration {
        public override void Up() {
            Create.Table("PREF_USERFILTER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("alias").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("application").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("schema").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("whereclause").AsString(MigrationUtil.StringSmall).NotNullable();
        }

        public override void Down() {

        }
    }
}