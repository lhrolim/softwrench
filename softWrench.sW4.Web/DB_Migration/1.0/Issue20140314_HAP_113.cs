using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._1._0 {

    [Migration(201403140838)]
    public class Issue20140314_HAP_113 : FluentMigrator.Migration {

        public override void Up() {
            Alter.Column("whereclause").OnTable("SW_DATACONSTRAINT").AsString(4000).NotNullable();
        }

        public override void Down() {
            // after db2 support the initial value was changed to AsString(4000) too
            Alter.Column("whereclause").OnTable("SW_DATACONSTRAINT").AsString(4000).NotNullable();
        }
    }
}