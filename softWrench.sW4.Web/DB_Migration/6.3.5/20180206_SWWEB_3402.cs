using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._6._3._5 {

    [Migration(201802061445)]
    public class _20180206_SWWEB_3402 : Migration {


        //https://logging.apache.org/log4net/release/config-examples.html

        public override void Up() {
            Create.Table("log_queries")
                    .WithColumn("id").AsInt64().PrimaryKey().Identity()
                    .WithColumn("date").AsDateTime().NotNullable()
                    .WithColumn("qualifier").AsString(255)
                    .WithColumn("query").AsString(4000)
                    .WithColumn("username").AsString(255)
                    .WithColumn("context_id").AsString(255)
                    .WithColumn("ellapsed").AsString(50);

        }

        public override void Down() {

        }
    }
}
