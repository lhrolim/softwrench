using System.Web.Razor.Generator;
using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration
{
    [Migration(201404151700)]
    public class Migration20140415Swweb193 : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("audit_ismtransaction")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("name").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("time").AsDateTime().NotNullable()
                .WithColumn("type").AsInt32().NotNullable()
                .WithColumn("routing").AsString(MigrationUtil.StringMedium).NotNullable();
        }

        public override void Down()
        {
            Delete.Table("audit_ismtransaction");
        
        }
    }
}