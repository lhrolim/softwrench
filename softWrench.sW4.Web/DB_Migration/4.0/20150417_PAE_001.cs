﻿using FluentMigrator;
using softWrench.sW4.Web.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace softWrench.sW4.Web.DB_Migration._4._0
{
    [Migration(201504170900)]
    public class Migration201500417PAE001 : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("AUDIT_ENTRY")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Action").AsString().NotNullable()
                .WithColumn("RefApplication").AsString().NotNullable()
                .WithColumn("RefId").AsInt64().NotNullable()
                .WithColumn("Data").AsString().NotNullable()
                .WithColumn("CreatedBy").AsString().NotNullable()
                .WithColumn("CreatedDate").AsDateTime().NotNullable();

            //Create.Table("SW_AUDITEVENT")
            //    .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            //    .WithColumn("Action").AsString().NotNullable()
            //    .WithColumn("RefEntity").AsString().NotNullable()
            //    .WithColumn("RefId").AsInt64().NotNullable()
            //    .WithColumn("CreatedBy").AsString().NotNullable()
            //    .WithColumn("CreatedDate").AsDateTime().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("AUDIT_ENTRY");
            //Delete.Table("SW_AUDITEVENT");
        }
    }
}