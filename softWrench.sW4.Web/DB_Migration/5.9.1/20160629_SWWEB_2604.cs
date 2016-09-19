using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._5._9._0 {
    [Migration(201606291631)]
    public class MigrationSwweb2604 : Migration {
        public override void Up() {
            IfDatabase("MySql").Execute.Sql("ALTER TABLE sw_metadataeditor MODIFY COLUMN metadata MEDIUMBLOB");

            IfDatabase("SqlServer").Execute.Sql("ALTER TABLE sw_metadataeditor ALTER COLUMN metadata varbinary(max)");
        }

        public override void Down() {
            
        }
    }
}
