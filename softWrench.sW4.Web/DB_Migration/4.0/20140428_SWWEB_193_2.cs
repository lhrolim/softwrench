using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201404281700)]
    public class Migration20140415Swweb193Part2 : FluentMigrator.Migration {
        public override void Up() {
            Delete.Table("audit_ismtransaction");

            Create.Table("audi_trail")
                .WithIdColumn()
                .WithColumn("name").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("begintime").AsDateTime().NotNullable()
                .WithColumn("endtime").AsDateTime().NotNullable();


            Create.Table("audi_ismtrail")
                .WithColumn("ISMTrailId").AsInt32().PrimaryKey().ForeignKey("audi_ismtrail_trail", "audi_trail", "id")
                .WithColumn("type").AsInt32().NotNullable()
                .WithColumn("routing").AsString(MigrationUtil.StringMedium).NotNullable();
        }

        public override void Down() {
            Delete.Table("audi_ismtrail");
            Delete.Table("audi_trail");
        }
    }

    [Migration(201405150707)]
    public class Migration20140415Swweb193Part3 : FluentMigrator.Migration {
        public override void Up() {
            Delete.Table("audi_ismtrail");
            Delete.Table("audi_trail");

            Create.Table("audi_trail")
                .WithIdColumn()
                .WithColumn("name").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("begintime").AsDateTime().NotNullable()
                .WithColumn("endtime").AsDateTime().NotNullable();

            Create.Table("audi_ismtrail")
               .WithColumn("ISMTrailId").AsInt32().PrimaryKey().ForeignKey("audi_ismtrail_trail", "audi_trail", "id")
               .WithColumn("type").AsInt32().NotNullable()
               .WithColumn("routing").AsString(MigrationUtil.StringMedium).NotNullable();
        }

        public override void Down() {
        }
    }
}