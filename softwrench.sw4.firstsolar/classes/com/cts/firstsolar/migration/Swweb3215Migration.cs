using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201710090745)]
    public class Swweb3215Migration : FluentMigrator.Migration {

        public override void Up()
        {
            Alter.Table("OPT_WORKPACKAGE").AddColumn("nerc").AsBoolean().WithDefaultValue(false);
        }

        public override void Down() {

        }
    }
}




