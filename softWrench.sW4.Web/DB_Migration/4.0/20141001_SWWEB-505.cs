using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201410011621)]
    public class Migration20141001Swweb505 : FluentMigrator.Migration {
        public override void Up()
        {

            Create.Table("PREF_GRIDFILTER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("alias_").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("application").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("schema_").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("fields").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("operators").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("values_").AsString(MigrationUtil.StringLarge).NotNullable()
                .WithColumn("creationdate").AsDateTime().NotNullable();

            Create.Table("PREF_GRIDFILTERASSOCIATION")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt32().ForeignKey("fk_user_id", "SW_USER2", "id")
                .WithColumn("gridfilter_id").AsInt32().ForeignKey("fk_gridfilter_id", "PREF_GRIDFILTER", "id")
                .WithColumn("creator").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("joiningdate").AsDateTime().NotNullable();

        }

        public override void Down() {
            Delete.Table("PREF_USERFILTER");
        }
    }
}