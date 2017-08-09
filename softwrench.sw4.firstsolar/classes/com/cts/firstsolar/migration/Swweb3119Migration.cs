using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201708091823)]
    public class Swweb3119Migration : FluentMigrator.Migration {

        public override void Up() {
            Alter.Table("OPT_WPEMAILSTATUS").AddColumn("cc").AsString(MigrationUtil.StringLarge).Nullable();
            Alter.Table("OPT_WORKPACKAGE").AddColumn("deleted").AsBoolean().WithDefaultValue(false);
        }

        public override void Down() {
        }
    }
}




