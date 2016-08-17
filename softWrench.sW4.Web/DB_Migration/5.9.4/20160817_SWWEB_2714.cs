using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._5._9._4 {
    [Migration(201608171417)]
    public class _20160817_SWWEB_2714 : Migration {
        public override void Up() {
            Alter.Table("SW_METADATAEDITOR").AddColumn("IPAddress").AsString(MigrationUtil.StringMedium).Nullable();
            Alter.Table("SW_METADATAEDITOR").AddColumn("ChangedByFullName").AsString(MigrationUtil.StringMedium).Nullable();
        }

        public override void Down() {

        }
    }
}
