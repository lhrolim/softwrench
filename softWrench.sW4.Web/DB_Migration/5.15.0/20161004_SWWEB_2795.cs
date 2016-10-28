using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._5._15._0 {
    [Migration(201610261725)]
    public class _20161004_SWWEB_2795 : Migration {
        public override void Up() {
            Create.Table("AUD_SESSION")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("UserId").AsInt64().Nullable()
                .WithColumn("StartDate").AsDateTime().Nullable()
                .WithColumn("EndDate").AsDateTime().Nullable();

            Create.Column("sessionid").OnTable("audi_trail").AsInt32().Nullable();
            Create.Column("operation").OnTable("audi_trail").AsString(MigrationUtil.StringLarge).Nullable();
        }

        public override void Down() {
            Delete.Table("AUD_SESSION");
            Delete.Column("sessionid").FromTable("audi_trail");
            Delete.Column("operation").FromTable("audi_trail");
        }
    }
}
