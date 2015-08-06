using FluentMigrator;

namespace softwrench.sW4.audit.Migration {
    [Migration(201508061215)]
    public class MigrationSwweb1322PartTwo : FluentMigrator.Migration {

        public override void Up() {
            Alter.Table("AUDIT_ENTRY").AlterColumn("RefUserId").AsString().WithDefaultValue("");
        }

        public override void Down() {
            
        }
    }
}
