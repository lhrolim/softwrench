using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {
    [Migration(201706201103)]
    public class Swweb3022Migration : Migration {

        public override void Up() {
            Create.Column("cc").OnTable("OPT_MAINTENANCE_ENG").AsString(MigrationUtil.StringLarge).Nullable();
        }

        public override void Down() {

        }

    }
}
