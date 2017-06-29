using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration.Batches {
    [Migration(201411031420)]

    public class Migration2014Swweb533 : FluentMigrator.Migration {


        public override void Up() {

            Create.Table("BAT_REPORT")
                .WithIdColumn(true)
                .WithColumn("SentItemIds").AsString(MigrationUtil.StringLarge).Nullable()
                .WithColumn("CreationDate").AsDateTime().NotNullable()
                .WithColumn("batch").AsInt64().ForeignKey("fk_report_batch", "BAT_BATCH", "Id");


            Create.Table("BAT_BATCHITEMPROBLEM")
                .WithIdColumn(true)
                .WithColumn("ErrorMessage").AsString(MigrationUtil.StringLarge).NotNullable()
                .WithColumn("itemid").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("report_id").AsInt64().ForeignKey("fk_problem_report", "BAT_REPORT", "Id")
                .WithColumn("DataMapJson").AsBinary().Nullable();
        }

        public override void Down() {
            Delete.Table("BAT_BATCHITEMPROBLEM");
            Delete.Table("BAT_REPORT");
        }
    }
}