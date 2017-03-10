using System;
using cts.commons.persistence.Util;
using cts.commons.portable.Util;
using FluentMigrator;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.DB_Migration._5._20._0 {

    [Migration(201703021631)]

    public class MigrationSwweb2929 : Migration {

        public override void Up()
        {


//            Delete.Table("DYN_SCRIPT_ENTRY");
            Delete.Column("Lastupdate").FromTable("DYN_SCRIPT_ENTRY");

            Alter.Table("DYN_SCRIPT_ENTRY").AddColumn("Lastupdate").AsInt64().WithDefaultValue(DateTime.Now.ToUnixTimeStamp());

            Create.Table("DYN_SCRIPT_JSENTRY")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Name").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("Target").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("Description").AsString(MigrationUtil.StringLarge).Nullable()
                .WithColumn("Script").AsString(MigrationUtil.StringMax).NotNullable()
                .WithColumn("Lastupdate").AsInt64().NotNullable()
                .WithColumn("Deploy").AsBoolean().WithDefaultValue(false)
                .WithColumn("Appliestoversion").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("platform").AsString(MigrationUtil.StringSmall).WithDefaultValue("both")
                .WithColumn("offlinedevice").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("offlineversions").AsString(MigrationUtil.StringSmall).Nullable();
        }

        public override void Down() {
            
        }
    }
}