using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201707251800)]
    public class Swweb2984Migration : Migration {

        public override void Up() {
            Delete.Column("sitename").FromTable("OPT_CALLOUT");
        }

        public override void Down() {
            Create.Column("sitename").OnTable("OPT_CALLOUT").AsString(MigrationUtil.StringMedium).Nullable();
        }
    }
}
