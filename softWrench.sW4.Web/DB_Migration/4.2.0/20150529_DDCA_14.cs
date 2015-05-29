using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._4._2._0 {

    [Migration(201505290925)]
    public class Migration20150529DDCA14 : FluentMigrator.Migration {

        public override void Up()
        {
            Alter.Table("SW_USER2")
                .AlterColumn("firstname").AsString().Nullable()
                .AlterColumn("lastname").AsString().Nullable();
        }

        public override void Down() {

        }
    }
}