using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201709181400)]
    public class Swweb3175Migration : FluentMigrator.Migration {

        public override void Up() {
            Alter.Table("OPT_DAILY_OUTAGE_ACTION").AlterColumn("action").AsString(MigrationUtil.StringLarge).NotNullable();
            Alter.Table("OPT_DAILY_OUTAGE_ACTION").AddColumn("duedate").AsDateTime().Nullable();
            Execute.Sql("update OPT_DAILY_OUTAGE_ACTION set duedate = actiontime");
        }

        public override void Down() {
            Delete.Column("duedate").FromTable("OPT_DAILY_OUTAGE_ACTION");
            Alter.Table("OPT_DAILY_OUTAGE_ACTION").AlterColumn("action").AsString().NotNullable();
        }
    }
}




