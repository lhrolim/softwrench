using cts.commons.persistence;
using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.migration {

    [Migration(201506062320)]
    public class Migration201506062320BatOffLine : Migration {
        public override void Up() {

            // on db2 is not possible to rename a referred table
            if (ApplicationConfiguration.IsDB2(DBType.Swdb)) {
                Delete.ForeignKey("fk_report_batch").OnTable("BAT_REPORT");
                Delete.ForeignKey("fk_batch_user_id").OnTable("BAT_BATCH");
            }

            Rename.Table("BAT_BATCH").To("BAT_MULBATCH");

            // on db2 is not possible to rename a referred table
            if (ApplicationConfiguration.IsDB2(DBType.Swdb)) {
                Create.ForeignKey("fk_report_batch")
                    .FromTable("BAT_REPORT")
                    .ForeignColumn("batch")
                    .ToTable("BAT_MULBATCH")
                    .PrimaryColumn("Id");
                Create.ForeignKey("fk_batch_user_id")
                    .FromTable("BAT_MULBATCH")
                    .ForeignColumn("userid")
                    .ToTable("SW_USER2")
                    .PrimaryColumn("id");
            }


            Create.Table("BAT_BATCH")
                .WithColumn("Id").AsInt64().PrimaryKey("pk_bat_batch2").Identity()
                .WithColumn("CreatedBy").AsInt32().ForeignKey("fk_batch2_user_id", "SW_USER2", "id")
                .WithColumn("CreationDate").AsDateTime().NotNullable()
                .WithColumn("Application").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("UpdateDate").AsDateTime().NotNullable()
                .WithColumn("Status").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("RemoteId").AsString(MigrationUtil.StringMedium).NotNullable();



            Create.Table("BAT_BATCHITEM")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("UpdateDate").AsDateTime().NotNullable()
                .WithColumn("Status").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("Application").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("ItemId").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("schema_").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("Operation").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("DataMapJson").AsBinary().NotNullable()
                .WithColumn("RemoteId").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("batch_id").AsInt64().ForeignKey("fk_batchitem_batch_id", "BAT_BATCH", "id")
                .WithColumn("problem_id").AsInt64().ForeignKey("fk_batchitem_problem_id", "PROB_PROBLEM", "id").Nullable();


        }

        public override void Down() {

        }
    }
}
