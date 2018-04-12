using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201804111400)]
    public class Migration20180411HAP1172 : FluentMigrator.Migration {

        public override void Up()
        {
            Create.Table("HIST_ASSETR0042")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("extractiondate").AsDateTime().NotNullable()
                .WithColumn("changedate").AsDateTime().NotNullable()
                .WithColumn("assetid").AsString(255).NotNullable()
                .WithColumn("assetnum").AsString(255).NotNullable()
                .WithColumn("itcname").AsString(255).Nullable()
                .WithColumn("userid").AsString(255).Nullable()
                .WithColumn("locdescription").AsString(4000).Nullable()
                .WithColumn("department").AsString(255).Nullable()
                .WithColumn("floor").AsString(255).Nullable()
                .WithColumn("room").AsString(255).Nullable()
                .WithColumn("serialnum").AsString(255).Nullable()
                .WithColumn("eosdate").AsString(255).Nullable()
                .WithColumn("usage").AsString(255).Nullable()
                .WithColumn("status").AsString(255).NotNullable()
                .WithColumn("macaddress").AsString(255).Nullable();


        }

        public override void Down() {

        }
    }
}