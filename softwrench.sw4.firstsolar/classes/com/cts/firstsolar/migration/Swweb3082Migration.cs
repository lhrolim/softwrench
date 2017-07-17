using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201707170100)]
    public class Swweb3082Migration : Migration {

        public override void Up() {
            Create.Column("cc").OnTable("OPT_DAILY_OUTAGE_MEETING").AsString().Nullable();
        }

        public override void Down() {
        }
    }
}
