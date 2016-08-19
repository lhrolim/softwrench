using FluentMigrator;

namespace softwrench.sw4.dashboard.classes.model.migration {
    [Migration(201608171900)]
    public class Migration20160817Swweb2603 : Migration {
        public override void Up() {
            Alter.Column("alias_").OnTable("dash_basepanel").AsString(100);
        }

        public override void Down() {
            Alter.Column("alias_").OnTable("dash_basepanel").AsString(30);
        }
    }
}