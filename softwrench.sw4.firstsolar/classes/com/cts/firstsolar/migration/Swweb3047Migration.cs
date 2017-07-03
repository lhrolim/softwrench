using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201706301946)]
    public class Swweb3047Migration : Migration {

        public override void Up() {
            Create.Column("mwhlostyesterday").OnTable("OPT_DAILY_OUTAGE_MEETING").AsDecimal().Nullable();

            Delete.Column("MwhLostTotal").FromTable("OPT_WORKPACKAGE");
        }

        public override void Down() {
        }
    }
}
