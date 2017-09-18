using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.offlineserver.migration {

    [Migration(201709171535)]
    public class MigrationSwoff258 : Migration {

        public override void Up() {
            Alter.Table("OFF_SYNCOPERATION")
                .AddColumn("ErrorMessage").AsString(MigrationUtil.StringLarge).Nullable()
                .AddColumn("StackTrace").AsString(MigrationUtil.StringMax).Nullable();
        }

        public override void Down() {

        }
    }
}
