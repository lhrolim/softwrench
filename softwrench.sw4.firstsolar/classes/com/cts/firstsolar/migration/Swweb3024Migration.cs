using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201706211800)]
    public class Swweb3024Migration : Migration {

        public override void Up() {
            Create.Column("estimatedcompdate").OnTable("OPT_WORKPACKAGE").AsDateTime().Nullable();
            Create.Column("actualcompdate").OnTable("OPT_WORKPACKAGE").AsDateTime().Nullable();
            Create.Column("mwhlosttotal").OnTable("OPT_WORKPACKAGE").AsString(MigrationUtil.StringSmall).Nullable();
            Create.Column("expectedmwhlost").OnTable("OPT_WORKPACKAGE").AsString(MigrationUtil.StringSmall).Nullable();
            Create.Column("mwhlostperday").OnTable("OPT_WORKPACKAGE").AsString(MigrationUtil.StringSmall).Nullable();
            Create.Column("problemstatement").OnTable("OPT_WORKPACKAGE").AsString(MigrationUtil.StringLarge).Nullable();

            Create.Table("OPT_DAILY_OUTAGE_MEETING")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("workpackageid").AsInt32().NotNullable()
            .WithColumn("meetingtime").AsDateTime().Nullable()
            .WithColumn("criticalpath").AsString(MigrationUtil.StringMedium).Nullable()
            .WithColumn("openactionitems").AsString(MigrationUtil.StringMedium).Nullable()
            .WithColumn("completedactionitems").AsString(MigrationUtil.StringMedium).Nullable()
            .WithColumn("summary").AsString(MigrationUtil.StringLarge).Nullable();
        }

        public override void Down() {
        }
    }
}
