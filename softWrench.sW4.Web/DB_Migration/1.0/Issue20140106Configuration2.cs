using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._1._0 {

    [Migration(201401091000)]
    public class Issue20140106Configuration2 : FluentMigrator.Migration {
        public override void Up()
        {
            Create.Column("blobvalue").OnTable("conf_propertyvalue").AsBinary().Nullable();
            Create.Column("defaultblobvalue").OnTable("conf_propertydefinition").AsBinary().Nullable();
            Alter.Column("defaultvalue").OnTable("conf_propertydefinition").AsString(4000).Nullable();
            Alter.Column("value").OnTable("conf_propertyvalue").AsString(4000).Nullable();
        }

        public override void Down() {

        }
    }
}