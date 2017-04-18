using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201704181400)]
    public class Migration20170418HAP1151 : FluentMigrator.Migration {
        public override void Up() {
            if (ApplicationConfiguration.ClientName == "hapag") {
                Alter.Table("SEC_PERSONGROUP").AddColumn("rowstamp").AsInt64().Nullable();
            }
        }

        public override void Down() {

        }
    }
}