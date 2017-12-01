using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201712011053)]
    public class Swweb3268Migration : FluentMigrator.Migration {

        public override void Up() {
            Execute.Sql(@"UPDATE [dbo].[OPT_WORKPACKAGE] SET [tier] = REPLACE(tier,'tier', '')");
        }

        public override void Down() {

        }
    }
}




