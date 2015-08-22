using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201502031205)]
    public class Migration20150203SWWEB698 : FluentMigrator.Migration {
        public override void Up() {
            // Updated the column to nvarchar without dropping the content
            Alter.Table("pref_gridfilter").AddColumn("Template").AsString(MigrationUtil.StringLarge).Nullable();
        }

        public override void Down() {

        }
    }
}
