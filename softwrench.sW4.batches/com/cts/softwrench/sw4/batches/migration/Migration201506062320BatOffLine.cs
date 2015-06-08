using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.migration {

    [Migration(201506062320)]
    public class Migration201506062320BatOffLine : Migration {
        public override void Up() {

            Rename.Table("BAT_BATCH").To("BAT_MULBATCH");

            Create.Table("BAT_BATCH")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
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
                .WithColumn("schema_").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("Operation").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("DataMapJson").AsBinary().NotNullable()
                .WithColumn("RemoteId").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("batch_id").AsInt64().ForeignKey("fk_batchitem_batch_id", "BAT_BATCH", "id")
                .WithColumn("problem_id").AsInt64().ForeignKey("fk_batchitem_problem_id", "PROB_PROBLEM", "id");


        }

        public override void Down() {

        }
    }
}
