using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._6._3._5 {

    [Migration(201802152145)]
    public class _20180215_SWWEB_3395 : Migration {



        public override void Up() {
            Alter.Table("CONF_PROPERTYVALUE").AddColumn("AllowCombining").AsBoolean().Nullable().WithDefaultValue(false);

        }

        public override void Down() {

        }
    }

    [Migration(201802212145)]
    public class _20180215_SWWEB_3395_2 : Migration {



        public override void Up() {
            Alter.Table("CONF_PROPERTYVALUE").AlterColumn("AllowCombining").AsBoolean().Nullable().WithDefaultValue(false);

        }

        public override void Down() {

        }
    }
}
