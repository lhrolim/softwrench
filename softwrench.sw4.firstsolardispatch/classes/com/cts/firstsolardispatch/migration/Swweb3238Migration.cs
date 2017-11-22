using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.migration {
    [Migration(201711221100)]
    public class Swweb3238Migration : Migration {

        public override void Up() {
            Alter.Table("GFED_SITE").AddColumn("maintenaceprovider").AsString(MigrationUtil.StringMedium).Nullable()
                .AddColumn("wherehouseaddress").AsString(MigrationUtil.StringMedium).Nullable();
            Alter.Table("DISP_TICKET").AddColumn("maintenaceprovider").AsString(MigrationUtil.StringMedium).Nullable();
            Alter.Table("DISP_INVERTER").AddColumn("failuredetails").AsString(MigrationUtil.StringLarge).Nullable();
        }

        public override void Down() {

        }

    }
}
