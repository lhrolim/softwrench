using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._4._2._0 {

    [Migration(201504221452)]
    public class Migration20150422Swweb1163 : FluentMigrator.Migration {

        public override void Up()
        {
            Alter.Table("CONF_WCCONDITION")
                .AddColumn("offlineonly").AsBoolean().NotNullable().WithDefaultValue(false);
        }

        public override void Down() {

        }
    }
}