using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._4._2._0 {

    [Migration(201606011329)]
    public class Migration20160601SWWEB2529 : Migration {

        public override void Up() {
            Alter.Table("PROB_PROBLEM").AddColumn("Problemtype").AsString(MigrationUtil.StringSmall);
            Alter.Table("PROB_PROBLEM").AddColumn("recorduserid").AsString(MigrationUtil.StringSmall);
            Alter.Table("PROB_PROBLEM").AddColumn("recordschema").AsString(MigrationUtil.StringSmall);
        }

        public override void Down() {

        }
    }
}