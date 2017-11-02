using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.migration {
    [Migration(201710191700)]
    public class Swweb3218Migration : Migration {

        public override void Up() {
            Create.Table("GFED_SITE")
                .WithIdColumn()
                .WithColumn("gfedid").AsInt64().NotNullable()
                .WithColumn("facilityname").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("facilitytitle").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("locationprefix").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("siteid").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("orgid").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("address").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("city").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("state").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("postalcode").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("country").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("sitecontact").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("sitecontactphone").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("supportphone").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("primarycontact").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("primarycontactphone").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("escalationcontact").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("escalationcontactphone").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("gpslatitude").AsDecimal(9, 6).Nullable()
                .WithColumn("gpslongitude").AsDecimal(9, 6).Nullable();


            Create.Table("DISP_PART_NEEDED")
                .WithIdColumn()
                .WithColumn("inverterid").AsInt32().NotNullable()
                .WithColumn("partnumber").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("partdescription").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("deliverymethod").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("expecteddate").AsDateTime().Nullable()
                .WithColumn("deliverylocation").AsString(MigrationUtil.StringMedium).Nullable();

            Create.Table("DISP_INVERTER")
                .WithIdColumn()
                .WithColumn("ticketid").AsInt32().NotNullable()
                .WithColumn("assetnum").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("siteid").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("manufacturer").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("model").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("errorcodes").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("failureclass").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("partsrequired").AsBoolean().NotNullable();

            Create.Table("DISP_TICKET")
                .WithIdColumn()
                .WithColumn("gfedid").AsInt64().NotNullable()
                .WithColumn("createddate").AsDateTime().NotNullable()
                .WithColumn("siteaddress").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("sitecontact").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("sitecontactphone").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("supportphone").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("primarycontact").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("primarycontactphone").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("escalationcontact").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("escalationcontactphone").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("gpslatitude").AsDecimal(9, 6).Nullable()
                .WithColumn("gpslongitude").AsDecimal(9, 6).Nullable()
                .WithColumn("comments").AsString(MigrationUtil.StringLarge).Nullable();

            Create.ForeignKey("fk_part_invert").FromTable("DISP_PART_NEEDED").ForeignColumn("inverterid").ToTable("DISP_INVERTER").PrimaryColumn("id");
            Create.ForeignKey("fk_invert_tic").FromTable("DISP_INVERTER").ForeignColumn("ticketid").ToTable("DISP_TICKET").PrimaryColumn("id");
        }

        public override void Down() {

        }

    }
}
