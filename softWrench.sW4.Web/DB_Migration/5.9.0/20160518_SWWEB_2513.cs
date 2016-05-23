using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._5._9._0 {
    [Migration(201605181730)]
    public class MigrationSwweb2513 : Migration {
        public override void Up() {
            Create.Column("cachedonclient").OnTable("CONF_PROPERTYDEFINITION").AsBoolean().WithDefaultValue(false);
        }

        public override void Down() {
            Delete.Column("cachedonclient").FromTable("CONF_PROPERTYDEFINITION");
        }
    }
}