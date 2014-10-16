﻿using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201410021630)]
    public class Migration201410021630SWWEB478 : FluentMigrator.Migration {
        public override void Up() {
            Create.Table("MAX_COMMREADFLAG")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Application").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("ApplicationItemId").AsInt64().NotNullable()
                .WithColumn("UserId").AsInt32().NotNullable()
                .WithColumn("CommlogId").AsInt64().NotNullable()
                .WithColumn("ReadFlag").AsBoolean().NotNullable();
        }

        public override void Down() {
            Delete.Table("MAX_COMMREADFLAG");
        }
    }
}