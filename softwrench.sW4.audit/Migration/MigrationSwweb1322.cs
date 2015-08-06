using FluentMigrator;

namespace softwrench.sW4.audit.Migration {
    [Migration(2015050817020)]
    public class MigrationSwweb1322 : FluentMigrator.Migration {

        public override void Up() {
            Alter.Table("AUDIT_ENTRY").AddColumn("RefUserId").AsString();
        }

        public override void Down() {
            Delete.Column("RefUserId").FromTable("AUDIT_ENTRY");
        }
    }
}
