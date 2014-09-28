using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201405051415)]
    public class Migration20140505Hap202 : FluentMigrator.Migration {
        public override void Up() {
            Create.Column("deletable").OnTable("SW_USERPROFILE").AsBoolean().WithDefaultValue(true);
            Create.Column("deletable").OnTable("SW_ROLE").AsBoolean().WithDefaultValue(true);
        }

        public override void Down() {
            Delete.Column("deletable").FromTable("SW_USERPROFILE");
            Delete.Column("deletable").FromTable("SW_ROLE");
        }
    }
}