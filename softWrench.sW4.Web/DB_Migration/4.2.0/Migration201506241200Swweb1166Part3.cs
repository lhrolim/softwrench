using FluentMigrator;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration._4._2._0 {
    [Migration(201506241200)]
    public class Migration201506241200Swweb1166Part3 : Migration {
        
        public override void Up() {
            Alter.Column("StackTrace").OnTable("PROB_PROBLEM").AsClob();
        }

        public override void Down() {
        }
    }
}