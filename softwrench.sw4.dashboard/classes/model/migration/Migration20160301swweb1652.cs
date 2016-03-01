using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.dashboard.classes.model.migration {
    [Migration(201603011315)]
    public class Migration20160301swweb1652 : Migration {

        public override void Up() {
            Alter.Column("configuration").OnTable("DASH_GRAPHICPANEL").AsClob();

        }

        public override void Down() {
            Delete.Table("DASH_GRAPHICPANEL");
        }
    }
}
