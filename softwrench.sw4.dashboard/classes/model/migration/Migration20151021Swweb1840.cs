using FluentMigrator;

namespace softwrench.sw4.dashboard.classes.model.migration {
    [Migration(201510220915)]
    public class Migration20151021Swweb1840 : Migration {

        public override void Up() {
            Create.Table("DASH_GRAPHICPANEL")
                .WithColumn("gpid").AsInt32().PrimaryKey().ForeignKey("dash_graphicpanel_parent", "DASH_BASEPANEL", "id")
                .WithColumn("configuration").AsString().NotNullable();
        }

        public override void Down() {
            Delete.Table("DASH_GRAPHICPANEL");
        }
    }
}
