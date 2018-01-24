using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201801241400)]
    public class Migration20180124HAP1173 : FluentMigrator.Migration {


        //https://logging.apache.org/log4net/release/config-examples.html

        public override void Up()
        {
            Create.Table("log_queries")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("date").AsDateTime().NotNullable()
                .WithColumn("qualifier").AsString(255)
                .WithColumn("query").AsString(4000)
                .WithColumn("username").AsString(255)
                .WithColumn("module").AsString(255)
                .WithColumn("ellapsed").AsString(50);

        }

        public override void Down() {

        }
    }
}