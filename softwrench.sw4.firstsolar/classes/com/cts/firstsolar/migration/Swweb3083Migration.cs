using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201707241045)]
    public class Swweb3083Migration : FluentMigrator.Migration {

        public override void Up() {
            Create.Table("OPT_DAILY_OUTAGE_ACTION").WithIdColumn()
                .WithColumn("action").AsString().NotNullable()
                .WithColumn("completed").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("actiontime").AsDateTime().NotNullable()
                .WithColumn("workpackageid").AsInt32().ForeignKey("fk_doa_wp", "OPT_WORKPACKAGE", "id").NotNullable();

            Create.ForeignKey("fk_dom_wp").FromTable("OPT_DAILY_OUTAGE_MEETING").ForeignColumn("workpackageid").ToTable("OPT_WORKPACKAGE").PrimaryColumn("id");
            Create.ForeignKey("fk_cal_wp").FromTable("OPT_CALLOUT").ForeignColumn("workpackageid").ToTable("OPT_WORKPACKAGE").PrimaryColumn("id");
            Create.ForeignKey("fk_meg_wp").FromTable("OPT_MAINTENANCE_ENG").ForeignColumn("workpackageid").ToTable("OPT_WORKPACKAGE").PrimaryColumn("id");

        }

        public override void Down() {
        }
    }

    [Migration(201708301045)]
    public class Swweb3147Migration : FluentMigrator.Migration {

        public override void Up()
        {
            Alter.Table("OPT_DAILY_OUTAGE_ACTION").AddColumn("assignee").AsString(MigrationUtil.StringMedium).NotNullable();
            Alter.Table("OPT_DAILY_OUTAGE_ACTION").AddColumn("assigneelabel").AsString(MigrationUtil.StringMedium).NotNullable();

        }

        public override void Down() {
        }
    }

    [Migration(201709090010)]
    public class Swweb3147_2Migration : FluentMigrator.Migration {
        public override void Up() {
            Alter.Column("assignee").OnTable("OPT_DAILY_OUTAGE_ACTION").AsString(MigrationUtil.StringMedium).Nullable();
            Alter.Column("assigneelabel").OnTable("OPT_DAILY_OUTAGE_ACTION").AsString(MigrationUtil.StringMedium).Nullable();
        }

        public override void Down() {
        }
    }
}




