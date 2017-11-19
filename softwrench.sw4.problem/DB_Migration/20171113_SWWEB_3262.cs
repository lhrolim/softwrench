using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sw4.problem.DB_Migration {

    [Migration(201711131621)]
    public class Migration20171113Swweb3262 : Migration {

        public override void Up() {
            Alter.Table("PROB_PROBLEM").AddColumn("ClientPlatform").AsString(MigrationUtil.StringSmall).Nullable();
            Alter.Table("PROB_PROBLEM").AddColumn("ReadOnly").AsBoolean().Nullable();
        }

        public override void Down() {

        }
    }
}