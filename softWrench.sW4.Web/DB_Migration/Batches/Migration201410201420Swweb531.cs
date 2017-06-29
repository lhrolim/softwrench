using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration.Batches {
    [Migration(201410201420)]

    public class Migration201410201420Swweb531 : FluentMigrator.Migration {


        public override void Up() {
            Create.Table("BAT_BATCH")
                .WithIdColumn(true)
                .WithColumn("Application").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("alias_").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("schema_").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("userid").AsInt32().ForeignKey("fk_batch_user_id", "SW_USER2", "id")
                .WithColumn("Status").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("CreationDate").AsDateTime().NotNullable()
                .WithColumn("UpdateDate").AsDateTime().NotNullable()
                .WithColumn("ItemIds").AsString(MigrationUtil.StringLarge).NotNullable()
                .WithColumn("DataMapJson").AsBinary().Nullable();
        }

        public override void Down() {
            Delete.Table("BAT_BATCH");
        }
    }
}