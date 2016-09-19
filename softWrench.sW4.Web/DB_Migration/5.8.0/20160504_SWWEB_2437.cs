using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._5._8._0 {
    [Migration(201605041220)]
    public class MigrationSwweb2437 : Migration {
        public override void Up() {
            Alter.Table("PREF_GRIDFILTER").AddColumn("advancedsearch").AsString(MigrationUtil.StringLarge).Nullable();

            // changing columns to nullable
            Alter.Column("fields").OnTable("PREF_GRIDFILTER").AsString(MigrationUtil.StringMedium).Nullable();
            Alter.Column("operators").OnTable("PREF_GRIDFILTER").AsString(MigrationUtil.StringMedium).Nullable();
            Alter.Column("values_").OnTable("PREF_GRIDFILTER").AsString(MigrationUtil.StringLarge).Nullable();
        }

        public override void Down() {
            Alter.Column("values_").OnTable("PREF_GRIDFILTER").AsString(MigrationUtil.StringLarge).NotNullable();
            Alter.Column("operators").OnTable("PREF_GRIDFILTER").AsString(MigrationUtil.StringMedium).NotNullable();
            Alter.Column("fields").OnTable("PREF_GRIDFILTER").AsString(MigrationUtil.StringMedium).NotNullable();

            Delete.Column("advancedsearch").FromTable("PREF_GRIDFILTER");
        }
    }
}