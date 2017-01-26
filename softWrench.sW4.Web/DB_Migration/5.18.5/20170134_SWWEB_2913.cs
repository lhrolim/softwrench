using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._5._18._5 {
    [Migration(201701241900)]
    public class MigrationSWWEB2913 : Migration {
        public override void Up() {
            Alter.Table("PREF_GRIDFILTER").AddColumn("sort").AsString(MigrationUtil.StringMedium).Nullable();
        }

        public override void Down() {
            Delete.Column("sort").FromTable("PREF_GRIDFILTER");
        }
    }
}