using cts.commons.persistence.Util;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201502261000)]
    public class Migration201502261000 : FluentMigrator.Migration {
        public override void Up() {
            // Updated the column to nvarchar without dropping the content
            Alter.Table("DASH_DASHBOARD").AlterColumn("USERPROFILES").AsString(MigrationUtil.StringMedium).Nullable();
            Alter.Table("DASH_BASEPANEL").AlterColumn("USERPROFILES").AsString(MigrationUtil.StringMedium).Nullable();
        }

        public override void Down() {

        }
    }
}
