using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.migration {
    [Migration(201712111413)]
    public class Swweb3293Migration : Migration {

        public override void Up() {

            Alter.Table("DISP_TICKET")
                .AddColumn("supportemail").AsString(MigrationUtil.StringMedium).Nullable();

            Alter.Table("GFED_SITE")
                .AddColumn("supportemail").AsString(MigrationUtil.StringMedium).Nullable();

            if (!ApplicationConfiguration.IsLocal()) {
                return;
            }

            Execute.Sql("UPDATE dbo.GFED_SITE SET supportemail = 'fssupport@power-electronics.com' where supportemail is null");
            Execute.Sql("UPDATE dbo.GFED_SITE SET supportphone = '866-794-7138' where (supportphone is null or supportphone like '(000); 000 0000')");

        }

        public override void Down() {

        }

    }
}
