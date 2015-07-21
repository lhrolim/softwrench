using FluentMigrator;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration._4._2._0 {
    [Migration(201507201700)]
    public class Migration201507201700Swweb1166Part4 : Migration {
        public override void Up() {
            Alter.Column("Message").OnTable("PROB_PROBLEM").AsClob();
        }

        public override void Down() {
        }
    }
}