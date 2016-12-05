using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._5._17 {
    [Migration(201612021200)]

    public class MigrationSwweb2879 : Migration {

        public override void Up() {
            Alter.Table("CONF_PROPERTYDEFINITION").AddColumn("minvalue_").AsInt32().Nullable();
            Alter.Table("CONF_PROPERTYDEFINITION").AddColumn("maxvalue_").AsInt32().Nullable();
        }

        public override void Down() {

        }
    }
}
