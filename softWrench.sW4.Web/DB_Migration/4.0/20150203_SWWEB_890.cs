using FluentMigrator;
using softWrench.sW4.Web.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace softWrench.sW4.Web.DB_Migration._4._0
{
      [Migration(201502031600)]
    public class Migration201502031600SWWEB890 : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("SW_METADATAEDITR")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Metadata").AsBinary().NotNullable()
                .WithColumn("CreatedDate").AsDateTime().NotNullable()
                .WithColumn("DefaultId").AsInt32().NotNullable();
                
        }

        public override void Down()
        {
            Delete.Table("SW_METADATAEDITR");
        }
    }
}