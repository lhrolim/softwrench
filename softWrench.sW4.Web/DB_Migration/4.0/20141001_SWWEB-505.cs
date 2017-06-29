using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201410011621)]
    public class Migration20141001Swweb505 : FluentMigrator.Migration {
        public override void Up()
        {

            Create.Table("PREF_GRIDFILTER")
                .WithIdColumn()
                .WithColumn("alias_").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("application").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("schema_").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("fields").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("operators").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("values_").AsString(MigrationUtil.StringLarge).NotNullable()
                .WithColumn("creationdate").AsDateTime().NotNullable()
                .WithColumn("updatedate").AsDateTime().Nullable()
                .WithColumn("creator").AsInt32().ForeignKey("fk_filter_user_id", "SW_USER2", "id");

            Create.Table("PREF_GRIDFILTERASSOCIATION")
                .WithIdColumn()
                .WithColumn("user_id").AsInt32().ForeignKey("fk_user_id", "SW_USER2", "id")
                .WithColumn("gridfilter_id").AsInt32().ForeignKey("fk_gridfilter_id", "PREF_GRIDFILTER", "id")
                .WithColumn("joiningdate").AsDateTime().NotNullable();
        }

        public override void Down() {
            Delete.Table("PREF_USERFILTER");
            Delete.Table("PREF_GRIDFILTERASSOCIATION");
        }
    }
}