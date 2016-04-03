using FluentMigrator;

namespace softwrench.sw4.dashboard.classes.model.migration {
    [Migration(201603311500)]
    public class Migration20160331Swweb2334 : Migration {
        public override void Up() {
            Alter.Table("DASH_DASHBOARD")
                .AddColumn("active").AsBoolean().NotNullable().WithDefaultValue(true);
        }

        public override void Down() {
            Delete.Column("active").FromTable("DASH_DASHBOARD");
        }
    }
}
