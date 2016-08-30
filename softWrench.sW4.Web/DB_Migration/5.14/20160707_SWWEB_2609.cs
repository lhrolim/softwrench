﻿using cts.commons.persistence;
using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.DB_Migration._5._14 {

    [Migration(201608250837)]
    public class MigrationSwweb2710 : Migration {
        public override void Up() {

            Create.Table("SW_MAPPING")
             .WithColumn("Id").AsInt64().PrimaryKey().Identity()
             .WithColumn("key").AsString(MigrationUtil.StringSmall).NotNullable()
             .WithColumn("originvalue").AsString(MigrationUtil.StringLarge).NotNullable()
             .WithColumn("destinationvalue").AsString(MigrationUtil.StringLarge).NotNullable();


            Create.Index("sw_mapping_key_idx").OnTable("SW_MAPPING").OnColumn("key");
            

        }

        public override void Down() {
        }
    }
}