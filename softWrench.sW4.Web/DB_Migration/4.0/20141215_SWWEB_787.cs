using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._4._0
{
    [Migration(201412151205)]
    public class Migration201410021630SWWEB787 : FluentMigrator.Migration
    {
        public override void Up()
        {
            // Updated the column to nvarchar without dropping the content
            Alter.Table("MAX_COMMREADFLAG").AlterColumn("ApplicationItemId").AsString(MigrationUtil.StringMedium).Nullable();
        }

        public override void Down()
        {

        }
    }
}
