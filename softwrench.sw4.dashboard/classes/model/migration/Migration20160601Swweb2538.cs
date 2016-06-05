using FluentMigrator;

namespace softwrench.sw4.dashboard.classes.model.migration {
    [Migration(201606011630)]
    public class Migration20160601Swweb2538 : Migration {
        public override void Up() {
            Alter.Column("title").OnTable("DASH_BASEPANEL").AsString(250);
        }

        public override void Down() {
            Alter.Column("title").OnTable("DASH_BASEPANEL").AsString(30);
        }
    }
}
