using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._5._10._0 {
    [Migration(201606011511)]
    public class MigrationSwweb2526 : Migration {
        public override void Up() {
            Create.Column("changepassword").OnTable("SW_USER2").AsBoolean().Nullable();
        }

        public override void Down() {
            Delete.Column("changepassword").FromTable("SW_USER2");
        }
    }
}