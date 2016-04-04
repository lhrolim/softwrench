using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.dashboard.classes.model.migration {
    [Migration(201604022155)]
    public class Migration20160402Swweb2315 : Migration {
        public override void Up() {
            Alter.Table("DASH_DASHBOARD")
                .AddColumn("system").AsBoolean().WithDefaultValue(false)
                .AddColumn("alias").AsString(MigrationUtil.StringSmall).Nullable()
                .AddColumn("application").AsString(MigrationUtil.StringSmall).Nullable()
                .AddColumn("preferredorder").AsInt32().Nullable();


            Update.Table("DASH_DASHBOARD").Set(new { alias = "workorder", system = true, application = "workorder" }).Where(new { title = "Work Orders" });
            Update.Table("DASH_DASHBOARD").Set(new { alias = "servicerequest", system = true, application = "servicerequest" }).Where(new { title = "Service Requests" });

        }

        public override void Down() {

        }
    }
}
