using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._5._11._1 {
    [Migration(201607071429)]
    public class MigrationSWWEB2609 : Migration {
        public override void Up() {
            Alter.Table("SEC_CONTAINER_PER").AddColumn("allowview").AsBoolean().WithDefaultValue(true);
        }

        public override void Down() {
        }
    }
}