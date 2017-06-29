using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.dashboard.classes.model.migration {
    [Migration(201502241200)]
    public class Migration20150224Swweb792 : FluentMigrator.Migration {
        public override void Up() {

            Create.Table("DASH_DASHBOARD").
                 WithIdColumn()
                .WithColumn("layout").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("title").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("userid").AsInt32().ForeignKey("fk_dashboard_user_id", "SW_USER2", "id").Nullable()
                .WithColumn("createdby").AsInt32().ForeignKey("fk_dashboardlc_user_id", "SW_USER2", "id")
                .WithColumn("creationdate").AsDateTime().NotNullable()
                .WithColumn("updatedate").AsDateTime().NotNullable()
                .WithColumn("userprofiles").AsString(MigrationUtil.StringMedium).Nullable();


            Create.Table("DASH_BASEPANEL").
                WithIdColumn()
                .WithColumn("alias_").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("title").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("userid").AsInt32().ForeignKey("fk_dashboardpanel_user_id", "SW_USER2", "id").Nullable()
                .WithColumn("createdby").AsInt32().ForeignKey("fk_dashboardpanelc_user_id", "SW_USER2", "id")
                .WithColumn("creationdate").AsDateTime().NotNullable()
                .WithColumn("updatedate").AsDateTime().NotNullable()
                .WithColumn("userprofiles").AsString(MigrationUtil.StringMedium).Nullable();

            Create.Table("DASH_GRIDPANEL").
                WithColumn("gpid").AsInt32().PrimaryKey().ForeignKey("dash_gridpanel_parent", "DASH_BASEPANEL", "id")
                .WithColumn("application").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("schemaref").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("fields").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("defaultsortfield").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("limit_").AsInt32().NotNullable().WithDefaultValue(30);


            Create.Table("DASH_DASHBOARDREL").
                WithIdColumn()
                .WithColumn("position").AsInt32().NotNullable()
                .WithColumn("panel_id").AsInt32().ForeignKey("dash_rel_base_panel", "DASH_BASEPANEL", "id").Nullable()
                .WithColumn("dashboard_id").AsInt32().ForeignKey("fk_dashboardrel_dash_id", "DASH_DASHBOARD", "id").Nullable();


            Create.Table("DASH_USERPREFERENCES").
                WithIdColumn()
                .WithColumn("preferred_id").AsInt32().ForeignKey("dash_preferences_dashboard", "DASH_DASHBOARD", "id");


        }

        public override void Down() {

        }
    }
}
