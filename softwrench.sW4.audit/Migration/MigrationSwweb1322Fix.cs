using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sW4.audit.Migration {
    [Migration(201710050955)]
    public class MigrationSwweb1322Fix : FluentMigrator.Migration {

        public override void Up() {
            Alter.Table("AUDIT_ENTRY").AlterColumn("RefId").AsInt64().Nullable();
            Alter.Table("AUDIT_ENTRY").AddColumn("siteid").AsString(MigrationUtil.StringSmall).Nullable();
        }

        public override void Down() {

        }
    }
}
