using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201407241400)]
    public class Migration201407241400SWWEB319 : FluentMigrator.Migration
    {
        public override void Up() {
            if (ApplicationConfiguration.ClientName == "hapag") {
                Create.Table("hist_ticket")
                    .WithIdColumn()
                    .WithColumn("ticketid").AsString(MigrationUtil.StringMedium).NotNullable()
                    .WithColumn("description").AsString(MigrationUtil.StringLarge).Nullable()
                    .WithColumn("reportedby").AsString(MigrationUtil.StringMedium).Nullable()
                    .WithColumn("affectedperson").AsString(MigrationUtil.StringMedium).Nullable()
                    .WithColumn("status").AsString(MigrationUtil.StringMedium).Nullable()
                    .WithColumn("assignedto").AsString(MigrationUtil.StringMedium).Nullable()
                    .WithColumn("ownergroup").AsString(MigrationUtil.StringMedium).Nullable()
                    .WithColumn("closedate").AsDateTime().Nullable()
                    .WithColumn("assetnum").AsString(MigrationUtil.StringMedium).Nullable()
                    .WithColumn("assetsiteid").AsString(MigrationUtil.StringMedium).Nullable()
                    .WithColumn("classification").AsString(MigrationUtil.StringMedium).Nullable()
                    .WithColumn("importdate").AsDateTime().Nullable();

                Create.Table("hist_workorder")
                    .WithIdColumn()
                    .WithColumn("wonum").AsString(MigrationUtil.StringMedium).NotNullable()
                    .WithColumn("description").AsString(MigrationUtil.StringLarge).Nullable()
                    .WithColumn("location").AsString(MigrationUtil.StringMedium).Nullable()
                    .WithColumn("assetnum").AsString(MigrationUtil.StringMedium).Nullable()
                    .WithColumn("status").AsString(MigrationUtil.StringMedium).Nullable()
                    .WithColumn("statusdate").AsDateTime().Nullable()
                    .WithColumn("importdate").AsDateTime().Nullable();
            }
        }

        public override void Down() {
            if (ApplicationConfiguration.ClientName == "hapag") {
                Delete.Table("hist_ticket");
                Delete.Table("hist_workorder");
            }
        }
    }
}