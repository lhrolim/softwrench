using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201708091823)]
    public class Swweb3119Migration : FluentMigrator.Migration {

        public override void Up() {
            Alter.Table("OPT_WPEMAILSTATUS").AddColumn("cc").AsString(MigrationUtil.StringLarge).Nullable();
            Alter.Table("OPT_WORKPACKAGE").AddColumn("deleted").AsBoolean().Nullable().WithDefaultValue(false);
        }

        public override void Down() {
        }
    }


    [Migration(201708111823)]
    public class Swweb3119Migration_2 : FluentMigrator.Migration {

        public override void Up() {
            Alter.Table("OPT_WORKPACKAGE").AlterColumn("deleted").AsBoolean().Nullable().WithDefaultValue(false);
        }

        public override void Down() {
        }
    }
}




