using FluentMigrator;
using softWrench.sW4.Extension;


namespace softWrench.sW4.Web.DB_Migration._4._0
{
    [Migration(201502051600)]
    public class Migration201502051600SWWEB889 : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("SW_METADATAEDITOR")
                .WithIdColumn(true)
                .WithColumn("Metadata").AsBinary().NotNullable()
                .WithColumn("Comments").AsString().NotNullable()
                .WithColumn("CreatedDate").AsDateTime().NotNullable()
                .WithColumn("DefaultId").AsInt32().NotNullable();

        }

        public override void Down()
        {
            Delete.Table("SW_METADATAEDITOR");
        }
    }
}