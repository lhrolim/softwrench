using cts.commons.persistence.Util;
using FluentMigrator;
using softwrench.sw4.api.classes.migration;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration._5._40._0 {
    [Migration(201710270630)]

    public class Migration20171027Swweb3238 : Migration {

        public override void Up()
        {


            Create.Table("SW_DOCINFO").WithIdColumn(true)
                .WithColumn("document").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("description").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("extension").AsString(MigrationUtil.StringSmall)
                .WithColumn("url").AsString(MigrationUtil.StringLarge).Nullable();
            

            Create.Table("SW_DOCLINK").WithIdColumn(true)
                .WithColumn("document").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("extension").AsString(MigrationUtil.StringSmall)
                .WithColumn("description").AsString(MigrationUtil.StringLarge).Nullable()
                .WithColumn("ownertable").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("ownerid").AsInt64().NotNullable()
                .WithColumn("createby").AsInt64().NotNullable()
                .WithColumn("createdate").AsDateTime().NotNullable()
                .WithColumn("docinfo_id").AsInt64().NotNullable().ForeignKey("sw_dl_di", "SW_DOCINFO", "id");

            Create.Table("SW_DOCLINK_QFR").WithIdColumn(true)
                .WithColumn("qualifier").AsString(MigrationUtil.StringMedium)
                .WithColumn("doclink_id").AsInt64().NotNullable().ForeignKey("sw_dlq_dl", "SW_DOCLINK", "id");





            if (MigrationContext.IsMySql) {
                Alter.Table("SW_DOCINFO").AddColumn("data").AsBinary().Nullable();
                Alter.Table("SW_DOCINFO").AddColumn("checksum").AsString(MigrationUtil.StringLarge);
            } else {
                Alter.Table("SW_DOCINFO").AddColumn("data").AsBinary((int.MaxValue)).Nullable();
                Alter.Table("SW_DOCINFO").AddColumn("checksum").AsString(MigrationUtil.StringLarge).Unique();
            }
        }

        public override void Down() {

        }
    }

    [Migration(20171130632)]

    public class Migration20171130Swweb32382 : Migration {

        public override void Up() {

            if (!MigrationContext.IsMySql)
            {
                Delete.Index("IX_SW_DOCINFO_checksum").OnTable("SW_DOCINFO");
//                Delete.Column("checksum").FromTable("SW_DOCINFO");
//                Alter.Table("SW_DOCINFO").AlterColumn("checksum").AsString(MigrationUtil.StringLarge);
            } 
        }

        public override void Down() {

        }
    }

}
