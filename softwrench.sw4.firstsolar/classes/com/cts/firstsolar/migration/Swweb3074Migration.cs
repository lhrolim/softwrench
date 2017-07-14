using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201707130100)]
    public class Swweb3074Migration : Migration {

        public override void Up() {
            Create.Column("sendnow").OnTable("OPT_DAILY_OUTAGE_MEETING").AsBoolean().WithDefaultValue(false);
            Create.Column("actualsendtime").OnTable("OPT_DAILY_OUTAGE_MEETING").AsDateTime().Nullable();
            Create.Column("status").OnTable("OPT_DAILY_OUTAGE_MEETING").AsString(MigrationUtil.StringSmall).Nullable();
        }

        public override void Down() {
        }
    }
}
