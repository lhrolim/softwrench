using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._5._9._0 {
    [Migration(201605171803)]
    public class _20160517_SWWEB_2492 : Migration {
        public override void Up() {
            Alter.Table("SW_METADATAEDITOR").AddColumn("Name").AsString(MigrationUtil.StringSmall).Nullable();
            Alter.Table("SW_METADATAEDITOR").AddColumn("Path").AsString(MigrationUtil.StringMedium).Nullable();
            Alter.Table("SW_METADATAEDITOR").AddColumn("BaselineVersion").AsString(MigrationUtil.StringSmall).Nullable();
            Alter.Table("SW_METADATAEDITOR").AddColumn("ChangedBy").AsString(MigrationUtil.StringSmall).Nullable();
        }

        public override void Down() {
        }
    }
}