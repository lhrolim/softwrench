using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._4._0 {

    [Migration(201405161700)]
    public class _20140516_SWWEB118_2 : FluentMigrator.Migration {

        public override void Up() {
                        
            Delete.Column("metadataid").FromTable("CONF_CONDITION");


            Alter.Table("CONF_WCCONDITION").AddColumn("metadataid").AsString(MigrationUtil.StringMedium).Nullable();
            
        }

        public override void Down() {

        }

    }
}