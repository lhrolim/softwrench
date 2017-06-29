using cts.commons.persistence.Util;
using FluentMigrator;
using softwrench.sw4.api.classes.migration;

namespace softWrench.sW4.Web.DB_Migration._5._19._0 {
    [Migration(201702101036)]

    public class MigrationSwweb2918 : Migration {

        public override void Up() {
            if (!MigrationContext.IsOracle) {
                Execute.Sql("delete from conf_propertydefinition where key_ = 'whereclause' and defaultvalue = '' and fullkey not in (select definition_id from conf_propertyvalue)");
            }

            Alter.Table("CONF_PROPERTYVALUE").AddColumn("clientname").AsString(MigrationUtil.StringSmall).Nullable();
        }

        public override void Down() {

        }
    }
}