using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.migration {
    [Migration(201711211000)]
    public class Swweb3224Migration : Migration {

        public override void Up() {
            Alter.Table("GFED_SITE")
                .AddColumn("primarycontactemail").AsString(MigrationUtil.StringMedium).Nullable()
                .AddColumn("escalationcontactemail").AsString(MigrationUtil.StringMedium).Nullable();

            Alter.Table("DISP_TICKET").AddColumn("accesstoken").AsString(MigrationUtil.StringMedium).Nullable();
        }

        public override void Down() {

        }

    }
}
